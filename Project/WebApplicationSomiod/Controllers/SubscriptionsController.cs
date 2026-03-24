using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Xml.Linq;
using WebApplicationSomiod.Utils;
using ModelsLibrary;

namespace WebApplicationSomiod.Controllers
{
    [RoutePrefix("api/somiod")]
    public class SubscriptionsController : ApiController
    {
        string connStr = Properties.Settings.Default.ConnectionStr;

        /// <summary>
        /// Get subscription
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="containerName">Container name</param>
        /// <param name="subName">Subscription name</param>
        /// <response code="404">Not Found</response>
        /// <returns></returns>
        [HttpGet, Route("{appName}/{containerName}/subs/{subName}")]
        public IHttpActionResult GetSubscription(string appName, string containerName, string subName)
        {
            Subscription sub = null;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand sqlCmd = new SqlCommand(@"
                    SELECT s.ResourceName, 
                        s.Evt, 
                        s.Endpoint, 
                        s.CreatedAt
                    FROM [Subscription] s 
                    JOIN [Container] c ON s.ContainerId = c.Id
                    JOIN [Application] a ON c.ApplicationId = a.Id
                    WHERE s.ResourceName = @subName
                        AND c.ResourceName = @containerName
                        AND a.ResourceName = @appName", conn);
                sqlCmd.Parameters.AddWithValue("@subName", subName);
                sqlCmd.Parameters.AddWithValue("@containerName", containerName);
                sqlCmd.Parameters.AddWithValue("@appName", appName);

                using (SqlDataReader reader = sqlCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        sub = new Subscription
                        {
                            ResType = "content-instance",
                            ResourceName = reader.GetString(0),
                            Evt = reader.GetInt32(1),
                            Endpoint = reader.GetString(2),
                            CreatedAt = reader.GetDateTime(3)
                        };
                    }
                }
            }

            if (sub == null)
                return NotFound();
            return Ok(sub);
        }

        /// <summary>
        /// Delete subscription
        /// </summary>
        /// <param name="appName">Application name</param>
        /// <param name="containerName">Container name</param>
        /// <param name="subName">Subscription name</param>
        /// <response code="404">Not Found</response>
        /// <returns></returns>
        [HttpDelete, Route("{appName}/{containerName}/subs/{subName}")]
        public IHttpActionResult DeleteSubscription(string appName, string containerName, string subName)
        {
            int? id = Id.GetSubscriptionId(appName, containerName, subName);
            if (id == null)
                return NotFound();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    DELETE FROM [Subscription]
                    WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);

                int rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                    return Ok();
                return NotFound();
            }
        }
    }
}
