namespace UbisamBase.Core.Backup;

public sealed class BackupItem
{
    public string Name { get; set; } = "새 항목";
    public bool Enabled { get; set; } = true;
    public string SourcePath { get; set; } = "";
    public string DestinationPath { get; set; } = "";
    public int KeepCount { get; set; } = 10;
}
