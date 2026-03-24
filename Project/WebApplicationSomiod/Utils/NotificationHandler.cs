using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using Newtonsoft.Json.Linq;

public class NotificationHandler
{

    public string HandleMessage(string message, string format, string evt)
    {

        string xmlLog = GenerateXmlLog(format, message, evt);
        if (ValidateXml(xmlLog))
        {
            SaveXml(xmlLog);
        }

        return xmlLog;
    }
    private string GenerateXmlLog(string format,  string content, string evt)
    {
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
        <notificationLog>
            <sourceFormat>{format}</sourceFormat>
            <evt>{evt}</evt>
            <content>{content}</content>
            <timestamp>{DateTime.Now:yyyy-MM-ddTHH:mm:ss}</timestamp>
        </notificationLog>";
    }


    private bool ValidateXml(string xmlContent)
    {
        try
        {
            string xsdPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Schemas",
                "NotificationLog.xsd"
            );

            if (!File.Exists(xsdPath))
                throw new FileNotFoundException("XSD não encontrado", xsdPath);

            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", xsdPath);

            XmlDocument doc = new XmlDocument();
            doc.Schemas = schemas;
            doc.LoadXml(xmlContent);

            doc.Validate(null);
            return true;
        }
        catch
        {
            return false;
        }
    }


    private void SaveXml(string xml)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string folder = Path.Combine(basePath, "Notifications");

        Directory.CreateDirectory(folder);

        string fileName = Path.Combine(
            folder,
            $"Handlernotification_{DateTime.Now:yyyyMMdd_HHmmss_fff}.xml"
        );

        File.WriteAllText(fileName, xml);
    }

}
