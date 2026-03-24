using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.UI.WebControls;
using System.Xml;
using uPLibrary.Networking.M2Mqtt;
using WebApplicationSomiod.Utils;
using ModelsLibrary;
using WebApplicationSomiod.Models;
using System.Reflection;
using System.Diagnostics;

namespace WebApplicationSomiod.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ContainersController : ApiController
    {
        string connStr = Properties.Settings.Default.ConnectionStr;

        /// <summary>
        /// Get container and discovery
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="containerName">Container name</param>
        /// <remarks>Optional header: somiod-discovery = content-instance | subscription</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <returns></returns>
        [HttpGet, Route("{appName}/{containerName}")]
        public IHttpActionResult GetContainer(string appName, string containerName)
        {
            //discovery
            var discovery = Request.Headers.Contains("somiod-discovery")
                ? Request.Headers.GetValues("somiod-discovery").First()
                : null;

            if (discovery != null)
            {
                var res = Discovery.Handler(discovery, appName, containerName);
                if (res != null)
                    return Ok(res);
                return BadRequest("Invalid discovery type.");
            }

            //get container
            Container container = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT c.ResourceName,
                        c.CreatedAt
                    FROM [Container] c
                    JOIN [Application] a ON c.ApplicationId = a.Id
                    WHERE a.ResourceName=@appName 
                        AND c.ResourceName=@containerName", conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@containerName", containerName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        container = new Container
                        {
                            ResType = "container",
                            ResourceName = reader.GetString(0),
                            CreatedAt = reader.GetDateTime(1)
                        };
                    }
                }
            }

            if (container == null)
                return NotFound();
            return Ok(container);
        }

        /// <summary>
        /// Update container
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="containerName">Container name</param>
        /// <param name="updated">Updated container data</param>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [HttpPut, Route("{appName}/{containerName}")]
        public IHttpActionResult UpdateContainer(string appName, string containerName, [FromBody] NewContainer updated)
        {   
            //body cant be empty
            if (updated == null)
                return BadRequest("Missing or invalid JSON body");

            //resource-name is required
            string resourceName = updated.ResourceName;
            if (string.IsNullOrEmpty(resourceName))
                return BadRequest("Missing or invalid resource-name");

            //get id
            int? containerId = Id.GetContainerId(appName, containerName);
            if (containerId == null)
                return NotFound();

            //check if it already exists
            if (Id.GetContainerId(appName, resourceName) != null)
                return Conflict();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    UPDATE [Container]
                    SET ResourceName=@ResourceName
                    WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", containerId);
                cmd.Parameters.AddWithValue("@ResourceName", resourceName);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok();
                return NotFound();
            }
        }

        /// <summary>
        /// Delete container
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="containerName">Container name</param>
        /// <response code="404">Not Found</response>
        /// <returns></returns>
        [HttpDelete, Route("{appName}/{containerName}")]
        public IHttpActionResult DeleteContainer(string appName, string containerName)
        {
            int? id = Id.GetContainerId(appName, containerName);
            if (id == null)
                return NotFound();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    DELETE FROM [Container]
                    WHERE Id=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok();
                return NotFound();
            }
        }

        /// <summary>
        /// Create subscription or content-instance
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="containerName">Container name</param>
        /// <param name="body">Data of the subscription/container</param>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [HttpPost, Route("{appName}/{containerName}")]
        public IHttpActionResult CreateOperation(string appName, string containerName, [FromBody] JObject body)
        {
            //body cant be empty
            if (body == null)
                return BadRequest("Missing or invalid JSON body");

            //res-type is required
            string resType = (string)body["res-type"];
            if (string.IsNullOrEmpty(resType))
                return BadRequest("Missing or invalid res-type");

            //get id
            int? containerId = Id.GetContainerId(appName, containerName);
            if (containerId == null)
                return NotFound();

            //get name
            string resourceName = (string)body["resource-name"];
            if (string.IsNullOrEmpty(resourceName))
                return BadRequest("Missing or invalid resource-name");

            //create according to type
            switch (resType)
            {
                case "subscription":
                    //check if it already exists
                    if (Id.GetSubscriptionId(appName, containerName, resourceName) != null)
                        return Conflict();

                    //get evt
                    int? evt = (int?)body["evt"];
                    if (evt == null || (evt != 1 && evt != 2))
                        return BadRequest("Missing or invalid evt");

                    //get endpoint (MQTT or HTTP). Validation is relaxed; format is checked when sending.
                    string endPoint = (string)body["endpoint"];
                    if (string.IsNullOrWhiteSpace(endPoint))
                        return BadRequest("Missing or invalid endpoint");

                    return CreateSubscription(containerId.Value, new Subscription
                    {
                        ResourceName = resourceName,
                        Evt = evt.Value,
                        Endpoint = endPoint
                    });

                case "content-instance":
                    //check if content-instance already exists
                    if (Id.GetCiId(appName, containerName, resourceName) != null)
                        return Conflict();

                    //get content-type
                    string contentType = (string)body["content-type"];
                    if (string.IsNullOrEmpty(contentType))
                        return BadRequest("Missing or invalid content-type");

                    //get content
                    string content = (string)body["content"];
                    if (string.IsNullOrEmpty(content))
                        return BadRequest("Missing or invalid content");

                    switch (contentType)
                    {
                        case "application/json":
                            try
                            {
                                JObject.Parse(content);
                            }
                            catch
                            {
                                return BadRequest("Invalid JSON content");
                            }
                            break;
                        case "application/xml":
                            try
                            {
                                new XmlDocument().LoadXml(content);
                            }
                            catch
                            {
                                return BadRequest("Invalid XML content");
                            }
                            break;
                        case "application/plaintext":
                            break;
                        default:
                            return BadRequest("Unsupported content-type");
                    }

                    return CreateContentInstance(appName, containerId.Value, containerName, new ContentInstance
                    {
                        ResourceName = resourceName,
                        ContentType = contentType,
                        Content = content
                    });
                    
                default:
                    return BadRequest("Invalid res-type");
            }
        }

        private IHttpActionResult CreateSubscription(int containerId, Subscription subscription)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO [Subscription] (ContainerId, ResourceName, Evt, EndPoint) 
                    VALUES (@ContainerId, @ResourceName, @Evt, @EndPoint)", conn);
                cmd.Parameters.AddWithValue("@ContainerId", containerId);
                cmd.Parameters.AddWithValue("@ResourceName", subscription.ResourceName);
                cmd.Parameters.AddWithValue("@Evt", subscription.Evt);
                cmd.Parameters.AddWithValue("@EndPoint", subscription.Endpoint);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok();
                return BadRequest();
            }
        }

        private IHttpActionResult CreateContentInstance(string applicationName, int containerId, string containerName, ContentInstance ci)
        {
            //insert in db
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO [ContentInstance] (ContainerId, ResourceName, ContentType, Content) 
                    VALUES (@ContainerId, @ResourceName, @ContentType, @Content)", conn);
                cmd.Parameters.AddWithValue("@ContainerId", containerId);
                cmd.Parameters.AddWithValue("@ResourceName", ci.ResourceName);
                cmd.Parameters.AddWithValue("@ContentType", ci.ContentType);
                cmd.Parameters.AddWithValue("@Content", ci.Content);

                int rows = cmd.ExecuteNonQuery();
                if (rows <= 0)
                    return BadRequest();
            }

            //get all endpoints subscribed to creation events
            List<string> endpoints = new List<string>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT Endpoint 
                    FROM [Subscription] 
                    WHERE ContainerId = @ContainerId 
                        AND Evt = 1", conn);
                cmd.Parameters.AddWithValue("@ContainerId", containerId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        endpoints.Add(reader.GetString(0));
                    }
                }
            }

            //build notification payload
            string notificationPayload = BuildNotificationPayload(
                "create",
                applicationName,
                containerName,
                ci
            );

            //publish to all endpoints
            foreach (string endpoint in endpoints)
            {
                //skip invalid endpoints
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

                        mClient.Publish(containerName + "/update/" + ci.ResourceName, Encoding.UTF8.GetBytes(ci.Content));
                    }
                    catch
                    {
                        //ignore failure
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
                            StringContent httpContent = new StringContent(notificationPayload, Encoding.UTF8, "application/json");
                            HttpResponseMessage response = httpClient.PostAsync(httpUri, httpContent).Result;
                        }
                    }
                    catch
                    {
                        //ignore failure
                    }
                }
            }

            //generate log
            GenerateAndSaveXmlLog(ci.ContentType, ci.Content);

            return Ok();
        }
        
        private void GenerateAndSaveXmlLog(string sourceFormat, string message)
        {
            NotificationHandler handler = new NotificationHandler();

            try
            { 
                string log = handler.HandleMessage(message, sourceFormat, "create");
                Console.WriteLine(log);
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message);
            }
        }

        private string BuildNotificationPayload(string evtType, string applicationName, string containerName, ContentInstance ci)
        {
            var payload = new JObject
            {
                ["evt"] = evtType,
                ["resource-name"] = ci.ResourceName,
                ["container-path"] = $"api/somiod/{applicationName}/{containerName}",
                ["content-type"] = ci.ContentType,
                ["content"] = ci.Content
            };

            return payload.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}