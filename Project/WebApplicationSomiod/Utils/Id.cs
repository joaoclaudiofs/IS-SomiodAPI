using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace WebApplicationSomiod.Utils
{
	public class Id
	{
        private static string connStr = Properties.Settings.Default.ConnectionStr;

        public static int? GetApplicationId(string appName)
		{
            int? id = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT Id 
                    FROM [Application]
                    WHERE ResourceName = @appName", conn);
                cmd.Parameters.AddWithValue("@appName", appName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                }

                return id;
            }
        }

        public static int? GetContainerId(string appName, string containerName)
        {
            int? id = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT c.Id 
                    FROM [Container] c
                    JOIN [Application] a ON c.ApplicationId = a.Id
                    WHERE a.ResourceName = @appName 
                        AND c.ResourceName = @containerName", conn);
                cmd.Parameters.AddWithValue("@containerName", containerName);
                cmd.Parameters.AddWithValue("@appName", appName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                }

                return id;
            }
        }

        public static int? GetCiId(string appName, string containerName, string ciName)
        {
            int? id = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT ci.Id 
                    FROM [ContentInstance] ci
                    JOIN [Container] c ON ci.ContainerId = c.Id
                    JOIN [Application] a ON c.ApplicationId = a.Id
                    WHERE a.ResourceName = @appName 
                        AND c.ResourceName = @containerName 
                        AND ci.ResourceName = @ciName", conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@containerName", containerName);
                cmd.Parameters.AddWithValue("@ciName", ciName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                }

                return id;
            }
        }

        public static int? GetSubscriptionId(string appName, string containerName, string subName)
        {
            int? id = null;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(@"
                    SELECT s.Id 
                    FROM [Subscription] s
                    JOIN [Container] c ON s.ContainerId = c.Id
                    JOIN [Application] a ON c.ApplicationId = a.Id
                    WHERE a.ResourceName = @appName 
                        AND c.ResourceName = @containerName 
                        AND s.ResourceName = @subName", conn);
                cmd.Parameters.AddWithValue("@appName", appName);
                cmd.Parameters.AddWithValue("@containerName", containerName);
                cmd.Parameters.AddWithValue("@subName", subName);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = reader.GetInt32(0);
                    }
                }

                return id;
            }
        }
    }
}