using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SomiodClient
{
    public class NotificationRecord
    {
        public string ResourcePath { get; set; }
        public string ResType { get; set; }
        public string ResourceName { get; set; }
        public int Event { get; set; }
        public string ContentType { get; set; }
        public string Content { get; set; }
        public string CreationDatetime { get; set; }
        public string RawBody { get; set; }
    }

    public class SaveResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
        public string SavedPath { get; set; }
    }

    public static class NotificationBuilder
    {
        public static NotificationRecord Build(string body, string contentType, string requestUrl)
        {
            var record = new NotificationRecord
            {
                ResourcePath = requestUrl ?? string.Empty,
                ResType = "unknown",
                ResourceName = "unknown",
                Event = 0,
                ContentType = contentType ?? string.Empty,
                Content = string.Empty,
                CreationDatetime = DateTime.UtcNow.ToString("s", CultureInfo.InvariantCulture),
                RawBody = body ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(body))
            {
                return record;
            }

            var isXml = contentType != null && contentType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) >= 0;
            if (isXml)
            {
                TryFillFromXml(body, record);
            }
            else
            {
                TryFillFromJson(body, record);
            }

            if (string.IsNullOrWhiteSpace(record.Content))
            {
                record.Content = body;
            }

            return record;
        }

        private static void TryFillFromJson(string body, NotificationRecord record)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                var dict = serializer.Deserialize<Dictionary<string, object>>(body);
                if (dict == null)
                {
                    return;
                }

                record.ResType = GetString(dict, "res-type", record.ResType);
                record.ResourceName = GetString(dict, "resource-name", record.ResourceName);
                record.ContentType = GetString(dict, "content-type", record.ContentType);
                record.Content = GetString(dict, "content", record.Content);
                record.Event = GetInt(dict, "evt", record.Event);
                record.CreationDatetime = GetString(dict, "creation-datetime", record.CreationDatetime);
                record.ResourcePath = GetString(dict, "resource-path", record.ResourcePath);
            }
            catch
            {
                // Swallow parse errors; fallback to raw body.
            }
        }

        private static void TryFillFromXml(string body, NotificationRecord record)
        {
            try
            {
                var doc = XDocument.Parse(body);
                record.ResType = GetElement(doc, "res-type", record.ResType);
                record.ResourceName = GetElement(doc, "resource-name", record.ResourceName);
                record.ContentType = GetElement(doc, "content-type", record.ContentType);
                record.Content = GetElement(doc, "content", record.Content);
                record.CreationDatetime = GetElement(doc, "creation-datetime", record.CreationDatetime);
                record.ResourcePath = GetElement(doc, "resource-path", record.ResourcePath);

                var evtText = GetElement(doc, "evt", record.Event.ToString(CultureInfo.InvariantCulture));
                if (int.TryParse(evtText, out var evtVal))
                {
                    record.Event = evtVal;
                }
            }
            catch
            {
                // Swallow parse errors; fallback to raw body.
            }
        }

        private static string GetString(Dictionary<string, object> dict, string key, string current)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                return Convert.ToString(dict[key], CultureInfo.InvariantCulture);
            }
            return current;
        }

        private static int GetInt(Dictionary<string, object> dict, string key, int current)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                int.TryParse(Convert.ToString(dict[key], CultureInfo.InvariantCulture), out current);
            }
            return current;
        }

        private static string GetElement(XDocument doc, string name, string current)
        {
            var el = doc.Root?.Element(name);
            return el != null ? el.Value : current;
        }
    }

    public static class NotificationXml
    {
        public static SaveResult SaveAndValidate(NotificationRecord record, string xmlPath, string xsdPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(xmlPath));
            var doc = BuildDocument(record);

            var validationMessage = Validate(doc, xsdPath);
            doc.Save(xmlPath);

            return new SaveResult
            {
                IsValid = validationMessage == null,
                ErrorMessage = validationMessage,
                SavedPath = xmlPath
            };
        }

        private static XDocument BuildDocument(NotificationRecord record)
        {
            return new XDocument(
                new XElement("notification",
                    new XElement("resource-path", record.ResourcePath ?? string.Empty),
                    new XElement("res-type", record.ResType ?? "unknown"),
                    new XElement("resource-name", record.ResourceName ?? "unknown"),
                    new XElement("evt", record.Event),
                    new XElement("content-type", record.ContentType ?? string.Empty),
                    new XElement("content", record.Content ?? string.Empty),
                    new XElement("creation-datetime", record.CreationDatetime ?? string.Empty),
                    new XElement("raw-body", record.RawBody ?? string.Empty)
                )
            );
        }

        private static string Validate(XDocument doc, string xsdPath)
        {
            if (!File.Exists(xsdPath))
            {
                return $"Schema not found at {xsdPath}";
            }

            var schemas = new XmlSchemaSet();
            schemas.Add(null, xsdPath);

            string validationMessage = null;
            doc.Validate(schemas, (o, e) =>
            {
                if (validationMessage == null)
                {
                    validationMessage = e.Message;
                }
            });

            return validationMessage;
        }
    }
}
