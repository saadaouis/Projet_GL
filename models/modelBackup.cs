using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace EasySave
{
    // Classe représentant une entrée de log
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string ProjectName { get; set; }
        public string BackupType { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public double Progress { get; set; }
    }

    public class ModelBackup
    {
        // Chemin absolu vers le fichier log (dans le dossier de l'exécutable)
        private readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "easysave_log.json");

        // Méthode asynchrone pour écrire une entrée dans le fichier log JSON
        private async Task WriteLogAsync(LogEntry entry)
        {
            List<LogEntry> logs = new List<LogEntry>();

            if (File.Exists(logFilePath))
            {
                string existingJson = await File.ReadAllTextAsync(logFilePath);
                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    logs = JsonSerializer.Deserialize<List<LogEntry>>(existingJson) ?? new List<LogEntry>();
                }
            }

            logs.Add(entry);

            var options = new JsonSerializerOptions { WriteIndented = true };
            string newJson = JsonSerializer.Serialize(logs, options);
            await File.WriteAllTextAsync(logFilePath, newJson);
        }

        // Simule une sauvegarde de projet avec rapport de progression et écrit les logs
        public async Task SaveProjectAsync(string projectName, bool isFullBackup, IProgress<double> progress)
        {
            try
            {
                await WriteLogAsync(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    ProjectName = projectName,
                    BackupType = isFullBackup ? "Complète" : "Différentielle",
                    Status = "En cours",
                    Message = "Sauvegarde démarrée",
                    Progress = 0.0
                });

                // Simulation de progression (10 étapes)
                for (int i = 1; i <= 10; i++)
                {
                    await Task.Delay(300); // simule le travail

                    double pct = i * 10;
                    progress.Report(pct);

                    await WriteLogAsync(new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        ProjectName = projectName,
                        BackupType = isFullBackup ? "Complète" : "Différentielle",
                        Status = "En cours",
                        Message = $"Sauvegarde à {pct}%",
                        Progress = pct
                    });
                }

                await WriteLogAsync(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    ProjectName = projectName,
                    BackupType = isFullBackup ? "Complète" : "Différentielle",
                    Status = "Terminé",
                    Message = "Sauvegarde réussie",
                    Progress = 100.0
                });
            }
            catch (Exception ex)
            {
                await WriteLogAsync(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    ProjectName = projectName,
                    BackupType = isFullBackup ? "Complète" : "Différentielle",
                    Status = "Erreur",
                    Message = ex.Message,
                    Progress = 0.0
                });
                throw; // relance l'exception après logging
            }
        }
    }
}
