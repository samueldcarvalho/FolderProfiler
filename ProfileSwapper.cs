using System.IO.Compression;

namespace FolderProfiler;

public sealed class ProfileSwapper
{
    private readonly SettingsService _settingsService;

    public ProfileSwapper(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void Swap(Settings settings, string targetProfile)
    {
        if (string.Equals(targetProfile, settings.ActualProfile, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Perfil alvo já é o perfil atual.");

        if (!settings.Profiles.Contains(targetProfile, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Perfil '{targetProfile}' não está na lista de profiles.");

        var actualProfile = settings.ActualProfile;
        var profiledFolder = Path.GetFullPath(settings.ProfiledFolderPath);

        if (!Directory.Exists(profiledFolder))
            throw new InvalidOperationException($"Pasta perfilada não existe: {profiledFolder}");

        var parentDirectory = Path.GetDirectoryName(profiledFolder);
        if (string.IsNullOrWhiteSpace(parentDirectory))
            throw new InvalidOperationException($"Não foi possível obter o diretório pai de: {profiledFolder}");

        var targetZip = _settingsService.GetProfileZipPath(settings, targetProfile);
        if (!File.Exists(targetZip))
            throw new InvalidOperationException(
                $"Zip do perfil '{targetProfile}' não encontrado:{Environment.NewLine}{targetZip}");

        var actualProfileDir = _settingsService.GetProfileDirectory(settings, actualProfile);
        var targetBackupDir = _settingsService.GetBackupDirectory(settings, targetProfile);
        Directory.CreateDirectory(actualProfileDir);
        Directory.CreateDirectory(targetBackupDir);

        var actualZip = _settingsService.GetProfileZipPath(settings, actualProfile);

        Cli.Step($"Compactando perfil atual '{actualProfile}'...");
        if (File.Exists(actualZip))
            File.Delete(actualZip);

        // Pasta perfilada fica na raíz do zip (ex.: .agents/...)
        ZipFile.CreateFromDirectory(profiledFolder, actualZip, CompressionLevel.Optimal, includeBaseDirectory: true);

        if (!File.Exists(actualZip) || new FileInfo(actualZip).Length <= 0)
            throw new InvalidOperationException("Falha ao criar o zip do perfil atual. Pasta não foi alterada.");

        Cli.Step("Zip salvo:");
        Cli.Path(actualZip);

        Cli.Step("Removendo pasta perfilada atual...");
        Directory.Delete(profiledFolder, recursive: true);

        Cli.Step($"Extraindo perfil '{targetProfile}'...");
        ExtractProfileZip(targetZip, profiledFolder, parentDirectory);

        if (!Directory.Exists(profiledFolder) || !Directory.EnumerateFileSystemEntries(profiledFolder).Any())
            throw new InvalidOperationException(
                $"Extração parece vazia. Zip do target permanece em:{Environment.NewLine}{targetZip}");

        var backupPath = BuildUniqueBackupPath(targetBackupDir);
        Cli.Step("Movendo zip do target para backup:");
        Cli.Path(backupPath);
        File.Move(targetZip, backupPath);

        settings.ActualProfile = targetProfile;
        _settingsService.Save(settings);
    }

    private static void ExtractProfileZip(string zipPath, string profiledFolder, string parentDirectory)
    {
        var folderName = Path.GetFileName(
            profiledFolder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        var zipHasProfileFolderAtRoot = ZipHasRootFolder(zipPath, folderName);
        if (zipHasProfileFolderAtRoot)
        {
            // Zip: .agents/...  → extrai no pai → pasta perfilada na raíz original
            ZipFile.ExtractToDirectory(zipPath, parentDirectory);
            return;
        }

        // Zip legado só com conteúdo (sem pasta na raíz) → recria a pasta e extrai dentro
        Directory.CreateDirectory(profiledFolder);
        ZipFile.ExtractToDirectory(zipPath, profiledFolder);
    }

    private static bool ZipHasRootFolder(string zipPath, string folderName)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        foreach (var entry in archive.Entries)
        {
            var name = entry.FullName.Replace('\\', '/').TrimStart('/');
            if (name.Length == 0)
                continue;

            var root = name.Split('/', 2)[0];
            if (string.Equals(root, folderName, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string BuildUniqueBackupPath(string backupDirectory)
    {
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var candidate = Path.Combine(backupDirectory, $"{stamp}.zip");
        if (!File.Exists(candidate))
            return candidate;

        var suffix = 2;
        while (true)
        {
            candidate = Path.Combine(backupDirectory, $"{stamp}_{suffix}.zip");
            if (!File.Exists(candidate))
                return candidate;
            suffix++;
        }
    }
}
