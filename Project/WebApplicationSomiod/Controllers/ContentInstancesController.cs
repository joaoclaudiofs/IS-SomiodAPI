using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using WebApplicationSomiod.Utils;
using ModelsLibrary;
using uPLibrary.Networking.M2Mqtt;

namespace WebApplicationSomiod.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ContentInstancesController : ApiController
    {
        string connStr = Properties.Settings.Default.ConnectionStr;

        /// <summary>
        /// Get content instance
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="containerName">Container name</param>
        /// <param name="ciName">Content instance name</param>
        /// <response code="404">Not Found</response>
        /// <returns></returns>
        [HttpGet, Route("{appName}/{containerName}/{ciName}")]
        public IHttpActionResult GetContentInstance(string appName, string containerName, string ciName)
        {
            ContentInstance ci = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand sqlCmd = new SqlCommand(@"
                    SELECT ci.ResourceName, 
                        ci.ContentType, 
                        ci.Content, 
                        ci.CreatedAt
                    FROM [ContentInstance] ci 
                    JOIN [Container] c ON ci.ContainerId = c.Id
                    JOIN [Application] a ON c.ApplicationId = a.Id
                    WHERE ci.ResourceName = @ciName
                        AND c.ResourceName = @containerName
                        AND a.ResourceName = @appName", conn);
                sqlCmd.Parameters.AddWithValue("@ciName", ciName);
                sqlCmd.Parameters.AddWithValue("@containerName", containerName);
                sqlCmd.Parameters.AddWithValue("@appName", appName);

                using (SqlDataReader reader = sqlCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ci = new ContentInstance
                        {
                            ResType = "content-instance",
                            ResourceName = reader.GetString(0),
                            ContentType = reader.GetString(1),
                            Content = reader.GetString(2),
                            CreatedAt = reader.GetDateTime(3)
                        };
                    }
                }
            }

            if (ci == null)
                return NotFound();
            return Ok(ci);
        }

        /// <summary>
        /// Delete content instance
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="containerName">Container name</param>
        /// <param name="ciName">Content instance name</param>
        /// <response code="404">Not Found</response>
        /// <returns></returns>
        [HttpDelete, Route("{appName}/{containerName}/{ciName}")]
        public IHttpActionResult DeleteContentInstance(string appName, string containerName, string ciName)
        {
            int? id = Id.GetCiId(appName, containerName, ciName);
            if (id == null)
                return NotFound();

            // collect endpoints subscribed to delete events
            List<string> endpoints = new List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT s.Endpoint
                    FROM [Subscription] s
                    JOIN [Container] c ON s.ContainerId = c.Id
                    JOIN [Application] a ON c.ApplicationId = a.Id
                    WHERE a.ResourceName = @appName
                        AND c.ResourceName = @containerName
                        AND s.Evt = 2", conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@containerName", containerName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        endpoints.Add(reader.GetString(0));
                    }
                }
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    DELETE FROM [ContentInstance]
                    WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    //build payload
                    string payload = BuildDeleteNotificationPayload(appName, containerName, ciName);

                    //publish to all endpoints (MQTT or HTTP)
                    foreach (string endpoint in endpoints)
                    {
                        if (string.IsNullOrWhiteSpace(endpoint))
                            continue;

                        //MQTT "mqtt://host:port"
                        if (endpoint.StartsWith("mqtt://"))
                        {
                            Uri uri = new Uri(endpoint);
                            string host = uri.Host;
                            int port = uri.Port;

                            try
                            {
                                MqttClient mClient = new MqttClient(host, port, false, MqttSslProtocols.None, null, null);
                                if (!mClient.IsConnected)
                                    mClient.Connect(Guid.NewGuid().ToString());

                                mClient.Publish(containerName, Encoding.UTF8.GetBytes(payload));
                            }
                            catch
                            {
                                //ignore failures
                            }

                            continue;
                        }

                        //HTTP
                        if (Uri.TryCreate(endpoint, UriKind.Absolute, out Uri httpUri) &&
                            (httpUri.Scheme == Uri.UriSchemeHttp || httpUri.Scheme == Uri.UriSchemeHttps))
                        {
                            try
                            {
                                using (HttpClient httpClient = new HttpClient())
                                {
                                    httpClient.DefaultRequestHeaders.Add("somiod-channel", containerName);
                                    StringContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                                    HttpResponseMessage response = httpClient.PostAsync(httpUri, httpContent).Result;
                                }
                            }
                            catch
                            {
                                //ignore failures
                            }
                        }
                    }

                    //generate log
                    GenerateAndSaveXmlLog("application/xml", payload);

                    return Ok();
                }

                return NotFound();
            }
        }

        private void GenerateAndSaveXmlLog(string sourceFormat, string message)
        {
            NotificationHandler handler = new NotificationHandler();

            try
            {
                string log = handler.HandleMessage(message, sourceFormat, "delete");
                Console.WriteLine(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string BuildDeleteNotificationPayload(string appName, string containerName, string ciName)
        {
            var payload = new Newtonsoft.Json.Linq.JObject
            {
                ["evt"] = "delete",
                ["res-type"] = "content-instance",
                ["resource-name"] = ciName,
                ["container-path"] = $"api/somiod/{appName}/{containerName}"
            };

            return payload.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
