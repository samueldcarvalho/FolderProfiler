using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FolderProfiler;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    public string RootDirectory { get; }
    public string SettingsPath { get; }

    public SettingsService()
    {
        RootDirectory = @"C:\FolderProfiler";
        SettingsPath = Path.Combine(RootDirectory, "settings.json");
    }

    public bool SettingsFileExists() => File.Exists(SettingsPath);

    public void CreateTemplate()
    {
        Directory.CreateDirectory(RootDirectory);

        var template = new Settings
        {
            ProjectName = "MeuJogo",
            ProfiledFolderPath = @"C:\caminho\para\profile",
            Profiles = ["eu", "mulher", "amigo"],
            ActualProfile = "eu"
        };

        var json = JsonSerializer.Serialize(template, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }

    public Settings Load()
    {
        var json = NormalizeWindowsPathsInJson(File.ReadAllText(SettingsPath));
        var settings = JsonSerializer.Deserialize<Settings>(json, JsonOptions)
            ?? throw new InvalidOperationException("Não foi possível ler o settings.json.");

        return settings;
    }

    /// <summary>
    /// Accepts hand-edited Windows paths with single backslashes (e.g. C:\Users\...)
    /// by turning invalid JSON escapes into \\ before System.Text.Json parses.
    /// </summary>
    internal static string NormalizeWindowsPathsInJson(string json)
    {
        var sb = new StringBuilder(json.Length + 16);
        var inString = false;

        for (var i = 0; i < json.Length; i++)
        {
            var c = json[i];

            if (!inString)
            {
                if (c == '"')
                    inString = true;
                sb.Append(c);
                continue;
            }

            if (c == '"')
            {
                inString = false;
                sb.Append(c);
                continue;
            }

            if (c != '\\')
            {
                sb.Append(c);
                continue;
            }

            if (i + 1 >= json.Length)
            {
                sb.Append("\\\\");
                continue;
            }

            var next = json[i + 1];
            if (next is '"' or '\\' or '/' or 'b' or 'f' or 'n' or 'r' or 't')
            {
                sb.Append('\\');
                sb.Append(next);
                i++;
                continue;
            }

            if (next == 'u'
                && i + 5 < json.Length
                && IsHex(json[i + 2])
                && IsHex(json[i + 3])
                && IsHex(json[i + 4])
                && IsHex(json[i + 5]))
            {
                sb.Append(json, i, 6);
                i += 5;
                continue;
            }

            sb.Append("\\\\");
        }

        return sb.ToString();
    }

    private static bool IsHex(char c) =>
        c is (>= '0' and <= '9') or (>= 'a' and <= 'f') or (>= 'A' and <= 'F');

    public void Save(Settings settings)
    {
        Directory.CreateDirectory(RootDirectory);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }

    public static string? Validate(Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ProjectName))
            return "projectName está vazio.";

        if (string.IsNullOrWhiteSpace(settings.ProfiledFolderPath))
            return "profiledFolderPath está vazio.";

        if (settings.Profiles is null || settings.Profiles.Count == 0)
            return "profiles está vazio.";

        if (string.IsNullOrWhiteSpace(settings.ActualProfile))
            return "actualProfile está vazio.";

        if (!settings.Profiles.Contains(settings.ActualProfile, StringComparer.OrdinalIgnoreCase))
            return $"actualProfile '{settings.ActualProfile}' não está em profiles.";

        var invalid = settings.Profiles
            .Where(p => string.IsNullOrWhiteSpace(p))
            .ToList();
        if (invalid.Count > 0)
            return "profiles contém nomes vazios.";

        return null;
    }

    public string GetProjectRoot(Settings settings) =>
        Path.Combine(RootDirectory, settings.ProjectName);

    public string GetFilesRoot(Settings settings) =>
        Path.Combine(GetProjectRoot(settings), "Files");

    public string GetProfileDirectory(Settings settings, string profile) =>
        Path.Combine(GetFilesRoot(settings), profile);

    public string GetProfileZipPath(Settings settings, string profile)
    {
        var folderName = GetFolderName(settings.ProfiledFolderPath);
        return Path.Combine(GetProfileDirectory(settings, profile), $"{folderName}_{profile}.zip");
    }

    public string GetBackupDirectory(Settings settings, string profile) =>
        Path.Combine(GetProfileDirectory(settings, profile), "backup");

    public static string GetFolderName(string profiledFolderPath) =>
        Path.GetFileName(profiledFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

    public void EnsureProjectLayout(Settings settings)
    {
        Directory.CreateDirectory(GetFilesRoot(settings));

        foreach (var profile in settings.Profiles)
        {
            Directory.CreateDirectory(GetProfileDirectory(settings, profile));
            Directory.CreateDirectory(GetBackupDirectory(settings, profile));
        }
    }
}
