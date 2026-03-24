using ModelsLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using static System.Net.Mime.MediaTypeNames;


namespace SomiodClient
{
    public partial class SomiodClientForm : Form
    {
        private SomiodApiClient apiClient = new SomiodApiClient(SomiodConstants.baseUrlSomiod);

        private MqttClient mqttClient;

        private bool isHandlingSubscription = false;

        public string containerName = "ac1";

        public string subscriptionName = "sub1";


        public SomiodClientForm()
        {
            InitializeComponent();
        }

        private void SomiodClientForm_Load(object sender, EventArgs e)
        {
            //criar a APP se não existir
            var ownAppResponse = apiClient.CreateApplication(SomiodConstants.applicationName);
            if (!ownAppResponse.IsSuccess && ownAppResponse.StatusCode != 409)
            {
                MessageBox.Show($"Failed to create controller application: {ownAppResponse.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //criar um container se não existir
            var ownContainerResponse = apiClient.CreateContainer(SomiodConstants.applicationName, containerName);
            if (!ownContainerResponse.IsSuccess && ownContainerResponse.StatusCode != 409)
            {
                MessageBox.Show($"Failed to search for devices: {ownContainerResponse.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //criar uma subscrição se não existir
            var ownSubscriptionResponse = apiClient.CreateSubscription(SomiodConstants.applicationName, containerName, subscriptionName);
            if (!ownSubscriptionResponse.IsSuccess && ownSubscriptionResponse.StatusCode != 409)
            {
                MessageBox.Show($"Failed to load subscriptions: {ownSubscriptionResponse.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            comboBoxContainers.Items.Add("N/A");
            comboBoxSubscriptions.Items.Add("N/A");
            RoundPanel(pnlStatus, 20);
            LoadContainers(sender, e);
            lblHouseName.Text = $"Available devices at {SomiodConstants.applicationName}";
            lblContainerName.Text = "Subscriptions for";
            lblACName.Text = "Temperature of";
            lblTemperature.Text = "OFF";
            lblStatus.Text = "Status: N/A";
            pnlStatus.BackColor = Color.WhiteSmoke;
        }

        private void SubscribeChannel(string ip, int port, string topic)
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }

            mqttClient = new MqttClient(ip, port, false, MqttSslProtocols.None, null, null);
            mqttClient.MqttMsgPublishReceived += MqttMessageReceived;
            mqttClient.Connect(Guid.NewGuid().ToString());

            mqttClient.Subscribe(
                new string[] { topic },
                new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE }
            );
        }

        private void MqttMessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Message);

            this.BeginInvoke(new Action(() =>
            {
                listBoxMessages.Items.Add($"{e.Topic}: {message}");
            }));


            HandleEvent(message);
        }

        private void HandleEvent(string message)
        {
            message = message.Trim();

            if (IsJson(message))
            {
                HandleJson(message);
            }
            else if (IsXml(message))
            {
                HandleXml(message);
            }
            else
            {
                try
                {
                    SavePlainTextToXml(message);
                    float temp = float.Parse(message);
                    UpdateStatus($"{temp} ºC");
                    UpdateInfo(temp);
                }
                catch
                {
                    UpdateStatus($"Invalid");
                    UpdateInfo(-1);
                }
            }
        }

        private bool IsXml(string message)
        {
            return message.StartsWith("<");
        }

        private bool IsJson(string message)
        {
            return message.StartsWith("{") || message.StartsWith("[");
        }

        private void HandleXml(string xml)
        {
            if (!ValidateXml(xml))
            {
                UpdateStatus("XML inválido (schema).");
                UpdateInfo(-1);
                return;
            }
            SaveXml(xml);
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(xml);

                var node = doc.SelectSingleNode("//temperature");

                if (node != null)
                {
                    if (float.TryParse(node.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out float temp))
                    {
                        UpdateStatus($"{temp} ºC");
                        UpdateInfo(temp);
                    }
                    else
                    {
                        UpdateStatus("Invalid temperature value");
                        UpdateInfo(-1);
                    }
                }
                else
                {
                    UpdateStatus("XML received but without temperature");
                    UpdateInfo(-1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error leading with XML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Failed to lead with XML");
                UpdateInfo(-1);
            }
        }

        private void HandleJson(string json)
        {
            SaveJsonToXml(json);
            try
            {
                var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
                var tempToken = obj.SelectToken("$..temperature");

                if (tempToken != null)
                {
                    float tempValue = (float)tempToken;
                    UpdateStatus($"{tempToken} ºC");
                    UpdateInfo(tempValue);
                }
                else
                {
                    UpdateStatus("JSON received but without temperature");
                    UpdateInfo(-1);
                }
            }
            catch
            {
                UpdateStatus("Invalid JSON received");
                UpdateInfo(-1);
            }
        }

        private bool ValidateXml(string xml)
        {
            try
            {
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add("", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Schemas\\Notification.xsd"));

                XmlDocument doc = new XmlDocument();
                doc.Schemas = schemas;
                doc.LoadXml(xml);

                doc.Validate((sender, e) =>
                {
                    throw new Exception(e.Message);
                });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Validation XML error: {ex.Message}", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void SaveXml(string xml)
        {
            string folder = "Notifications";
            Directory.CreateDirectory(folder);

            string fileName = $"{folder}/notification_{DateTime.Now:yyyyMMdd_HHmmss_fff}.xml";
            File.WriteAllText(fileName, xml);
        }

        private void SaveJsonToXml(string json)
        {
            try
            {
                var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
                var tempToken = obj.SelectToken("$..temperature");

                if (tempToken == null)
                    throw new Exception("JSON without temperature");

                string temperature = tempToken.ToString();

                XmlDocument doc = new XmlDocument();

                XmlElement root = doc.CreateElement("notification");
                XmlElement tempElem = doc.CreateElement("temperature");
                tempElem.InnerText = temperature;

                root.AppendChild(tempElem);
                doc.AppendChild(root);

                if (!ValidateXml(doc.OuterXml))
                {
                    UpdateStatus("Invalid XML (schema)");
                    UpdateInfo(-1);
                    return;
                }
                SaveXml(doc.OuterXml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving JSON to XML: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void SavePlainTextToXml(string text)
        {
            try
            {
                if (!float.TryParse(
                        text,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out float temperature))
                {
                    throw new Exception("Plain text without a valid temperature");
                }

                XmlDocument doc = new XmlDocument();

                XmlElement root = doc.CreateElement("notification");
                XmlElement tempElem = doc.CreateElement("temperature");
                tempElem.InnerText = temperature.ToString(CultureInfo.InvariantCulture);

                root.AppendChild(tempElem);
                doc.AppendChild(root);

                if (!ValidateXml(doc.OuterXml))
                {
                    UpdateStatus("Invalid XML (schema");
                    UpdateInfo(-1);
                    return;
                }
                SaveXml(doc.OuterXml);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving plain text to XML: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void UpdateStatus(string text)
        {
            if (lblTemperature.InvokeRequired)
            {
                lblTemperature.BeginInvoke(new Action(() =>
                {
                    lblTemperature.Text = text;
                }));
            }
            else
            {
                lblTemperature.Text = text;
            }
        }

        private void UpdateInfo(float temp)
        {
            if (lblStatus.InvokeRequired)
            {
                lblStatus.BeginInvoke(new Action(() => UpdateInfo(temp)));
                return;
            }

            if (temp == -1)
            {
                lblStatus.Text = "Status: N/A";
                pnlStatus.BackColor = Color.Gray;
            }
            else if (temp < 20)
            {
                lblStatus.Text = "Status: Cold";
                pnlStatus.BackColor = Color.FromArgb(192, 192, 255);
            }
            else if (temp <= 25)
            {
                lblStatus.Text = "Status: OK";
                pnlStatus.BackColor = Color.LightGreen;
            }
            else
            {
                lblStatus.Text = "Status: Warm";
                pnlStatus.BackColor = Color.IndianRed;
            }
        }

        private void LoadContainers(object sender, EventArgs e)
        {
            try
            {
                string url = $"{SomiodConstants.baseUrlSomiod}/{SomiodConstants.applicationName}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("somiod-discovery", "container");

                var response = apiClient.GetApplicationContainers(url);
                if (!response.IsSuccess)
                {
                    listBoxMessages.Items.Add("Failed to fetch containers.");
                    return;
                }

                var containers = response.Data;

                comboBoxContainers.Items.Clear();

                string containerName = "";
                foreach (var containerPath in containers)
                {
                    containerName = containerPath.Replace("/api/somiod/" + SomiodConstants.applicationName + "/", "");
                    comboBoxContainers.Items.Add(containerName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading containers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSubscriptions(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxContainers.SelectedItem == null)
                    return;

                string selectedContainerName = comboBoxContainers.SelectedItem?.ToString() ?? "";

                string url = $"{SomiodConstants.baseUrlSomiod}/{SomiodConstants.applicationName}/{selectedContainerName}";

                var response = apiClient.GetContainerSubscriptions(url);
                if (!response.IsSuccess)
                {
                    listBoxMessages.Items.Add(
                        $"Failed to load subscriptions for {selectedContainerName}."
                    );
                    return;
                }
                var subscriptions = response.Data;

                comboBoxSubscriptions.Items.Clear();

                if(subscriptions.Count == 0)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        comboBoxSubscriptions.Text = "N/A";
                    }));

                    return;
                }

                foreach (var sub in subscriptions)
                {
                    string subName = sub.Replace(
                        "/api/somiod/" + SomiodConstants.applicationName + "/" + selectedContainerName + "/",
                        ""
                    );

                    this.BeginInvoke(new Action(() =>
                    {
                        comboBoxSubscriptions.Items.Add(subName/* + "(U)"*/);
                    }));
                
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading subscriptions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBoxSubscriptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeSubscription(sender, e);
        }
        private async void ChangeSubscription(object sender, EventArgs e)
        {
            if (isHandlingSubscription)
                return;

            if (comboBoxSubscriptions.SelectedItem == null)
                return;

            isHandlingSubscription = true;

            try
            {
                string selectedSub = comboBoxSubscriptions.SelectedItem.ToString();
                    /*.Replace("(U)", "")
                    .Replace("(S)", "")
                    .Trim();*/

                string appName = SomiodConstants.applicationName;
                string containerName = comboBoxContainers.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(containerName))
                    return;

                string url = $"{SomiodConstants.baseUrlSomiod}/{appName}/{containerName}/{selectedSub}";

                listBoxMessages.Items.Add($"Fetching subscription '{selectedSub}'...");

                var response = await Task.Run(() => apiClient.GetSubscription(url));

                if (!response.IsSuccess || string.IsNullOrWhiteSpace(response.Data))
                {
                    listBoxMessages.Items.Add($"Failed to get endpoint for subscription '{selectedSub}'");
                    return;
                }

                JObject json;
                try
                {
                    json = JObject.Parse(response.Data);
                }
                catch
                {
                    listBoxMessages.Items.Add("Invalid JSON returned by subscription.");
                    return;
                }
                
                //prevent evt2 
                if (json["evt"] != null && (int)json["evt"] != 1)
                {
                    listBoxMessages.Items.Add($"Subscription '{selectedSub}' isn't supported (evt = {json["evt"]}).");
                    return;
                }

                string endpoint = json["endpoint"]?.ToString();

                if (string.IsNullOrWhiteSpace(endpoint))
                {
                    listBoxMessages.Items.Add($"Subscription '{selectedSub}' has no endpoint.");
                    return;
                }

                Uri uri = new Uri(endpoint);

                string ip = uri.Host; 
                int port = uri.Port;   
                string channel = comboBoxContainers.SelectedItem.ToString();

                SubscribeChannel(ip, port, channel);
                listBoxMessages.Items.Add($"Subscribing to topic '{channel}' on {ip}:{port}");

                //editSubscriptionText("(S)");

                listBoxMessages.Items.Add($"Subscribed successfully to '{selectedSub}'");
            }
            catch (Exception ex)
            {
                listBoxMessages.Items.Add($"Error subscribing: {ex.Message}");
            }
            finally
            {
                isHandlingSubscription = false;
            }
        }

        private void comboBoxContainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*string selectedContainer = comboBoxContainers.SelectedItem?.ToString() ?? "";
            lblContainerName.Text = $"Subscriptions for {selectedContainer}";
            comboBoxSubscriptions.Items.Clear();
            comboBoxSubscriptions.Text = "";
            LoadSubscriptions(sender, e);
            lblACName.Text = $"Temperature of {selectedContainer}";
            foreach (var topic in subscribedTopics.ToList())
            {
                unsubscribeTopic(topic);
            }
            subscribedTopics.Clear();*/
            unsubscribeChannel();

            comboBoxSubscriptions.Items.Clear();
            comboBoxSubscriptions.Text = "";
            LoadSubscriptions(sender, e);

            string selectedContainer = comboBoxContainers.SelectedItem?.ToString() ?? "";
            lblContainerName.Text = $"Subscriptions for {selectedContainer}";
            lblACName.Text = $"Temperature of {selectedContainer}";
            lblTemperature.Text = "OFF";
            lblStatus.Text = "Status: N/A";
            pnlStatus.BackColor = Color.WhiteSmoke;
        }

        private void RoundPanel(Panel panel, int radius)
        {
            Rectangle bounds = panel.ClientRectangle;
            int diameter = radius * 2;

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            //canto superior esquerdo
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            //topo
            path.AddLine(bounds.X + radius, bounds.Y, bounds.Right - radius, bounds.Y);
            //canto superior direito
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            //direita
            path.AddLine(bounds.Right, bounds.Y + radius, bounds.Right, bounds.Bottom - radius);
            //canto inferior direito
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            //fundo
            path.AddLine(bounds.Right - radius, bounds.Bottom, bounds.X + radius, bounds.Bottom);
            //canto inferior esquerdo
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            //esquerda
            path.AddLine(bounds.X, bounds.Bottom - radius, bounds.X, bounds.Y + radius);

            path.CloseFigure();

            panel.Region = new Region(path);
        }

        private void btnStopSub_Click(object sender, EventArgs e)
        {
            /*foreach (var topic in subscribedTopics.ToList())
            {
                unsubscribeTopic(topic);
            }
            for(int i = 0; i < comboBoxSubscriptions.Items.Count; i++)
            {
                string sub = comboBoxSubscriptions.Items[i].ToString();
                sub = sub.Replace("(U)", "").Replace("(S)", "").Trim();
                comboBoxSubscriptions.Items[i] = sub + "(U)";
            }
            subscribedTopics.Clear();*/
            if(comboBoxSubscriptions.SelectedItem == null)
            {
                MessageBox.Show("No chanel subscribed", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            comboBoxSubscriptions.Text = "";
            lblTemperature.Text = "OFF";
            lblStatus.Text = "Status: N/A";
            pnlStatus.BackColor = Color.WhiteSmoke;
            unsubscribeChannel();
        }

        private void unsubscribeTopic(string topic)
        {
            if (string.IsNullOrEmpty(topic))
            {
                MessageBox.Show("No topic subscribed", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (mqttClient == null || !mqttClient.IsConnected)
            {
                MessageBox.Show("MQTT off", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            mqttClient.Unsubscribe(new string[] { topic });

            editSubscriptionText("(U)");

            MessageBox.Show($"Unsubscribed from '{topic}'");


            comboBoxSubscriptions.SelectedIndex = -1;
            listBoxMessages.Items.Clear();
        }

        private void unsubscribeChannel()
        {
            if (mqttClient == null)
                return;

            if (mqttClient.IsConnected)
            {
                mqttClient.Disconnect(); 
                MessageBox.Show($"Unsubscribed from chanels");
                listBoxMessages.Items.Clear();
            }
               

        }

        public void editSubscriptionText(string text)
        {
            int index = comboBoxSubscriptions.SelectedIndex;

            if (index >= 0)
            {
                string sub = comboBoxSubscriptions.Items[index].ToString();
                sub = sub.Replace("(U)", "").Replace("(S)", "").Trim();
                comboBoxSubscriptions.Items[index] = sub + text;
            }
        }

        private void SomiodClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }
        }

        private void listBoxMessages_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
