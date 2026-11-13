namespace FolderProfiler;

public sealed class Settings
{
    public string ProjectName { get; set; } = "";
    public string ProfiledFolderPath { get; set; } = "";
    public List<string> Profiles { get; set; } = [];
    public string ActualProfile { get; set; } = "";
}
