using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.IO;
using System.Xml;
using RestSharp;
using ModelsLibrary;


namespace SomiodClient2
{
    public partial class SomiodClient2Form : Form
    {
        private string baseURL = @"http://localhost:44316/api/somiod";
        private string appName = "a";
        private string ownAppName = "controller";
        private SomiodApiClient apiClient;

        public SomiodClient2Form()
        {
            InitializeComponent();
            apiClient = new SomiodApiClient(baseURL);
        }

        private void SomiodClient2Form_Load(object sender, EventArgs e)
        {
            labelTemp.Text = trackBarTemp.Value.ToString() + " °C";
            
            // Set JSON as default format
            checkBoxJson.Checked = true;
            
            // Create own application (controller) if it doesn't exist
            var ownAppResponse = apiClient.CreateApplication(ownAppName);
            if (!ownAppResponse.IsSuccess && ownAppResponse.StatusCode != 409)
            {
                MessageBox.Show($"Failed to create controller application: {ownAppResponse.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var containersResponse = apiClient.GetApplicationContainers(appName);
            
            if (containersResponse.IsSuccess && containersResponse.Data != null)
            {
                listBoxContainers.Items.Clear();
                foreach (string path in containersResponse.Data)
                {
                    string trimPath = path.Replace("/api/somiod/" + appName + "/", "");
                    listBoxContainers.Items.Add(trimPath);
                }
            }
            else
            {
                MessageBox.Show($"Failed to load containers from '{appName}': {containersResponse.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void trackBarTemp_Scroll(object sender, EventArgs e)
        {
            labelTemp.Text = trackBarTemp.Value.ToString() + " °C";
        }

        private void buttonPostCI_Click(object sender, EventArgs e)
        {
            // Post a content instance to the selected container
            if (listBoxContainers.SelectedItem == null)
            {
                MessageBox.Show("Please select a container from the list.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedContainer = listBoxContainers.SelectedItem.ToString();
            string ciContent;
            string contentType;

            // Determine content format based on selected checkbox
            if (checkBoxJson.Checked)
            {
                ciContent = $"{{\"temperature\": {trackBarTemp.Value}}}";
                contentType = "application/json";
            }
            else if (checkBoxXML.Checked)
            {
                ciContent = $"<notification><temperature>{trackBarTemp.Value}</temperature></notification>";
                contentType = "application/xml";
            }
            else if (checkBoxPlainText.Checked)
            {
                ciContent = trackBarTemp.Value.ToString();
                contentType = "application/plaintext";
            }
            else
            {
                MessageBox.Show("Please select a content format (JSON, XML, or Plain Text).", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ContentInstance ci = new ContentInstance
            {
                ResType = "content-instance",
                ResourceName = $"ci_{DateTime.Now.Ticks}",
                ContentType = contentType,
                Content = ciContent
            };

            var ciResponse = apiClient.CreateContentInstance(appName, selectedContainer, ci);
            if (ciResponse.IsSuccess)
            {
                listBoxOutput.Items.Clear();
                listBoxOutput.Items.Add(ciContent);
            }
            else
            {
                listBoxOutput.Items.Add($"[{DateTime.Now:HH:mm:ss}] Error: Failed to post content");
                MessageBox.Show($"Failed to post Content Instance: {ciResponse.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkBoxJson_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxJson.Checked)
            {
                checkBoxXML.Checked = false;
                checkBoxPlainText.Checked = false;
            }
        }

        private void checkBoxXML_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxXML.Checked)
            {
                checkBoxJson.Checked = false;
                checkBoxPlainText.Checked = false;
            }
        }

        private void checkBoxPlainText_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPlainText.Checked)
            {
                checkBoxJson.Checked = false;
                checkBoxXML.Checked = false;
            }
        }
    }
}
