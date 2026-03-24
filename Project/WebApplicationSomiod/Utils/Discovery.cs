using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using WebApplicationSomiod.Models;

namespace WebApplicationSomiod.Utils
{
    public class Discovery
    {
        private static string connStr = Properties.Settings.Default.ConnectionStr;

        public static List<String> Handler(string discovery, string appName = null, string containerName = null)
        {
            switch (discovery)
            {
                case "application":
                    return Applications();
                case "container":
                    return Containers(appName);
                case "content-instance":
                    return ContentInstances(appName, containerName);
                case "subscription":
                    return Subscriptions(appName, containerName);
                default:
                    return null;
            }
        }

        private static List<string> Applications()
        {
            List<string> paths = new List<string>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT ResourceName FROM [Application]", conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paths.Add($"/api/somiod/{reader.GetString(0)}");
                    }
                }
            }

            return paths;
        }

        private static List<string> Containers(string appName)
        {
            List<string> paths = new List<string>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string sql = @"
                    SELECT a.ResourceName, 
                        c.ResourceName 
                    FROM [Container] c
                    JOIN [Application] a ON c.ApplicationId = a.Id";

                if (appName != null)
                {
                    sql += $" WHERE a.ResourceName = '{appName}'";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paths.Add($"/api/somiod/{reader.GetString(0)}/{reader.GetString(1)}");
                    }
                }

                return paths;
            }
        }

        private static List<string> ContentInstances(string appName, string containerName)
        {
            List<string> paths = new List<string>();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string sql = @"
                    SELECT a.ResourceName, 
                        c.ResourceName, 
                        ci.ResourceName 
                    FROM [ContentInstance] ci
                    JOIN [Container] c ON ci.ContainerId = c.Id
                    JOIN [Application] a ON c.ApplicationId = a.Id";

                if (appName != null)
                {
                    sql += $" WHERE a.ResourceName = '{appName}'";
                }
                if (containerName != null)
                {
                    sql += $" AND c.ResourceName = '{containerName}'";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paths.Add($"/api/somiod/{reader.GetString(0)}/{reader.GetString(1)}/{reader.GetString(2)}");
                    }
                }

                return paths;
            }
        }

        private static List<string> Subscriptions(string appName, string containerName)
        {
            List<string> paths = new List<string>();

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                var sql = @"
                    SELECT a.ResourceName, 
                        c.ResourceName, 
                        s.ResourceName 
                    FROM [Subscription] s
                    JOIN [Container] c ON s.ContainerId = c.Id
                    JOIN [Application] a ON c.ApplicationId = a.Id";

                if (appName != null)
                {   
                    sql += $" WHERE a.ResourceName = '{appName}'";
                }
                if (containerName != null)
                {
                    sql += $" AND c.ResourceName = '{containerName}'";
                }

                var cmd = new SqlCommand(sql, conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paths.Add($"/api/somiod/{reader.GetString(0)}/{reader.GetString(1)}/subs/{reader.GetString(2)}");
                    }
                }

                return paths;
            }
        }
    }
}