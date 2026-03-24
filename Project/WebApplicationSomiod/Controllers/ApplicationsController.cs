using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using WebApplicationSomiod.Utils;
using ModelsLibrary;
using WebApplicationSomiod.Models;

namespace WebApplicationSomiod.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ApplicationsController : ApiController
    {
        string connStr = Properties.Settings.Default.ConnectionStr;

        /// <summary>
        /// Discovery
        /// </summary>
        /// <remarks>Required header: somiod-discovery = application | container | content-instance | subscription</remarks>
        /// <response code="400">Bad Request</response>
        /// <returns></returns>
        [HttpGet, Route("")]
        public IHttpActionResult Discover()
        {
            //discovery
            var discovery = Request.Headers.Contains("somiod-discovery")
                ? Request.Headers.GetValues("somiod-discovery").First()
                : null;

            if (discovery != null)
            {
                var res = Discovery.Handler(discovery);
                if (res != null)
                    return Ok(res);
                return BadRequest("Invalid discovery type");
            }

            //invalid request
            return BadRequest("Missing 'somiod-discovery' header");
        }

        /// <summary>
        /// Create application
        /// </summary>
        /// <param name="newApp">New application data</param>
        /// <response code="400">Bad Request</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [HttpPost, Route("")]
        public IHttpActionResult CreateApplication([FromBody] NewApplication newApp)
        {
            //body cant be empty
            if (newApp == null)
                return BadRequest("Missing or invalid JSON body");

            //res-type is required and must be "application"
            string resType = newApp.ResType;
            if (string.IsNullOrEmpty(resType) || resType != "application")
                return BadRequest("Missing or invalid res-type");

            //resource-name is required
            string resourceName = newApp.ResourceName;
            if (string.IsNullOrEmpty(resourceName))
                return BadRequest("Missing or invalid resource-name");

            //check if it already exists
            if (Id.GetApplicationId(resourceName) != null)
                return Conflict();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO [Application] (ResourceName) 
                    VALUES (@ResourceName)", conn);
                cmd.Parameters.AddWithValue("@ResourceName", resourceName);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok();
                return BadRequest();
            }
        }

        /// <summary>
        /// Get application and discovery
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <remarks>Optional header: somiod-discovery = container | content-instance | subscription</remarks>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <returns></returns>
        [HttpGet, Route("{appName}")]
        public IHttpActionResult GetApplication(string appName)
        {
            //discovery
            var discovery = Request.Headers.Contains("somiod-discovery")
                ? Request.Headers.GetValues("somiod-discovery").First()
                : null;

            if (discovery != null)
            {
                var res = Discovery.Handler(discovery, appName);
                if (res != null)
                    return Ok(res);
                return BadRequest("Invalid discovery type");
            }

            //get application
            Application app = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT ResourceName,
                        CreatedAt       
                    FROM [Application] 
                    WHERE ResourceName=@appName", conn);
                cmd.Parameters.AddWithValue("@appName", appName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        app = new Application {
                            ResType = "application",
                            ResourceName = reader.GetString(0),
                            CreatedAt = reader.GetDateTime(1)
                        };
                    }
                }
            }

            if (app == null) return NotFound();
            return Ok(app);
        }

        /// <summary>
        /// Update application
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="updated">Updated application data</param>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [HttpPut, Route("{appName}")]
        public IHttpActionResult UpdateApplication(string appName, [FromBody] NewApplication updated)
        {
            //body cant be empty
            if (updated == null)
                return BadRequest("Missing or invalid JSON body");

            //resource-name is required
            string resourceName = updated.ResourceName;
            if (string.IsNullOrEmpty(resourceName))
                return BadRequest("Missing or invalid resource-name");

            //get id
            int? applicationId = Id.GetApplicationId(appName);
            if (applicationId == null)
                return NotFound();

            //check if it already exists
            if (Id.GetApplicationId(resourceName) != null)
                return Conflict();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    UPDATE [Application]
                    SET ResourceName=@ResourceName
                    WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", applicationId);
                cmd.Parameters.AddWithValue("@ResourceName", resourceName);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok();
                return NotFound();
            }
        }

        /// <summary>
        /// Delete application
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <response code="404">Not Found</response>
        /// <returns></returns>
        [HttpDelete, Route("{appName}")]
        public IHttpActionResult DeleteApplication(string appName)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    DELETE FROM [Application] 
                    WHERE ResourceName=@appName", conn);
                cmd.Parameters.AddWithValue("@appName", appName);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok();
                return NotFound();
            }
        }

        /// <summary>
        /// Create container
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="newContainer">New container data</param>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <response code="409">Conflict</response>
        /// <returns></returns>
        [HttpPost, Route("{appName}")]
        public IHttpActionResult CreateContainer(string appName, [FromBody] NewContainer newContainer)
        {
            //body cant be empty
            if (newContainer == null)
                return BadRequest("Missing or invalid JSON body");

            //res-type is required and must be "container"
            string resType = newContainer.ResType;
            if (string.IsNullOrEmpty(resType) || resType != "container")
                return BadRequest("Missing or invalid res-type");

            //resource-name is required
            string resourceName = newContainer.ResourceName;
            if (string.IsNullOrEmpty(resourceName))
                return BadRequest("Missing or invalid resource-name");

            //check if it already exists
            if (Id.GetContainerId(appName, resourceName) != null)
                return Conflict();

            //get id
            int? applicationId = Id.GetApplicationId(appName);
            if (applicationId == null)
                return NotFound();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO [Container] (ApplicationId, ResourceName) 
                    VALUES (@ApplicationId, @ResourceName)", conn);
                cmd.Parameters.AddWithValue("@ApplicationId", applicationId);
                cmd.Parameters.AddWithValue("@ResourceName", resourceName);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok();
                return BadRequest();
            }
        }
    }
}
