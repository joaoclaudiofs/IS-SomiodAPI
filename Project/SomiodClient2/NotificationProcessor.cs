using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

public class NotificationProcessor
{
    private readonly string _schemaPath;
    private readonly string _outDir;

    public NotificationProcessor()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        _schemaPath = Path.Combine(baseDir, "Schemas", "notification.xsd");
        _outDir = Path.Combine(baseDir, "Notifications");
        Directory.CreateDirectory(_outDir);
    }

    public (bool IsValid, string MessageShort) ProcessNotification(string xml)
    {
        try
        {
            // Save raw for debugging
            var dt = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            var filename = Path.Combine(_outDir, $"notification_{dt}.xml");
            File.WriteAllText(filename, xml);

            // validate
            if (!File.Exists(_schemaPath))
            {
                return (false, "Schema not found: " + _schemaPath);
            }

            var settings = new XmlReaderSettings();
            settings.Schemas.Add(null, _schemaPath);
            settings.ValidationType = ValidationType.Schema;
            string validationMsg = null;
            settings.ValidationEventHandler += (s, e) => { validationMsg = e.Message; };

            using (var sr = new StringReader(xml))
            using (var xr = XmlReader.Create(sr, settings))
            {
                while (xr.Read()) { }
            }

            if (validationMsg != null)
            {
                File.AppendAllText(filename, "\n<!-- Validation Error: " + validationMsg + " -->");
                return (false, "Invalid XML: " + validationMsg);
            }

            return (true, "Valid notification saved");
        }
        catch (Exception ex)
        {
            return (false, "Processing error: " + ex.Message);
        }
    }
}
