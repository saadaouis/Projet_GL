using System;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace EasySave.Logging
{
    public class Logger
    {
        private string logFilePath;
        private string logTextFilePath;
        private string logXmlFilePath;

        public Logger()
        {
            string baseDir = AppContext.BaseDirectory;
            logFilePath = Path.Combine(baseDir, "easysave_logs.json");
            logTextFilePath = Path.Combine(baseDir, "easysave_logs.txt");
            logXmlFilePath = Path.Combine(baseDir, "easysave_logs.xml");

            // Crée le fichier XML si besoin
            if (!File.Exists(logXmlFilePath))
            {
                using (var writer = XmlWriter.Create(logXmlFilePath, new XmlWriterSettings { Indent = true }))
                {
                    writer.WriteStartElement("Logs");
                    writer.WriteEndElement();
                }
            }
        }

        public void SetLogFilePath(string path) => logFilePath = path;
        public void SetLogTextFilePath(string path) => logTextFilePath = path;
        public void SetLogXmlFilePath(string path) => logXmlFilePath = path;

        public void Log(string message)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:dd/MM/yyyy HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(logTextFilePath, logMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'écriture du log texte : " + ex.Message);
            }
        }

        public void LogJson(string name, string fileSource, string fileTarget, long fileSize, double fileTransferTime)
        {
            var logEntry = new
            {
                Name = name,
                FileSource = fileSource,
                FileTarget = fileTarget,
                FileSize = fileSize,
                FileTransferTime = fileTransferTime,
                Time = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")
            };

            try
            {
                string json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = true });

                if (!File.Exists(logFilePath))
                {
                    File.WriteAllText(logFilePath, "[\n" + json + "\n]");
                }
                else
                {
                    var content = File.ReadAllText(logFilePath).TrimEnd();
                    if (content.EndsWith("]"))
                    {
                        content = content.Substring(0, content.Length - 1);
                        content += ",\n" + json + "\n]";
                        File.WriteAllText(logFilePath, content);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'écriture du log JSON : " + ex.Message);
            }
        }

        public void LogXml(string name, string fileSource, string fileTarget, long fileSize, double fileTransferTime)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(logXmlFilePath);

                XmlNode logNode = doc.CreateElement("Log");

                void AddElement(string name, string value)
                {
                    XmlNode node = doc.CreateElement(name);
                    node.InnerText = value;
                    logNode.AppendChild(node);
                }

                AddElement("Name", name);
                AddElement("FileSource", fileSource);
                AddElement("FileTarget", fileTarget);
                AddElement("FileSize", fileSize.ToString());
                AddElement("FileTransferTime", fileTransferTime.ToString("F2"));
                AddElement("Time", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                doc.DocumentElement.AppendChild(logNode);
                doc.Save(logXmlFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'écriture du log XML : " + ex.Message);
            }
        }
    }
}
