namespace PomodoroWidget.Models;

public class DataStatus
{
    public string DataFilePath { get; set; } = string.Empty;
    public string BackupDirectoryPath { get; set; } = string.Empty;
    public string HealthText { get; set; } = "Unknown";
    public string LastSavedText { get; set; } = "Not saved yet";
    public string LatestBackupText { get; set; } = "No backups yet";
    public string DataFileSizeText { get; set; } = "0 KB";
    public string LatestBackupSizeText { get; set; } = "0 KB";
    public int DataVersion { get; set; }
    public int BackupCount { get; set; }
    public bool DataFileExists { get; set; }
    public bool BackupDirectoryExists { get; set; }
}
