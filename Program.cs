using FolderProfiler;

Console.Title = "FolderProfiler";

var settingsService = new SettingsService();

if (!settingsService.SettingsFileExists())
{
    settingsService.CreateTemplate();
    Cli.Title("FolderProfiler");
    Cli.Info("Nenhuma configuração encontrada.");
    Cli.Blank();
    Cli.Info("Foi criado um settings.json base. Edite o arquivo e execute novamente:");
    Cli.Path(settingsService.SettingsPath);
    Cli.WaitForExit();
    return;
}

Settings settings;
try
{
    settings = settingsService.Load();
}
catch (Exception ex)
{
    Cli.Title("FolderProfiler");
    Cli.Error($"Erro ao ler settings.json: {ex.Message}");
    Cli.Path(settingsService.SettingsPath);
    Cli.WaitForExit();
    return;
}

var validationError = SettingsService.Validate(settings);
if (validationError is not null)
{
    Cli.Title("FolderProfiler");
    Cli.Error($"Configuração inválida: {validationError}");
    Cli.Info("Corrija o arquivo e execute novamente:");
    Cli.Path(settingsService.SettingsPath);
    Cli.WaitForExit();
    return;
}

settingsService.EnsureProjectLayout(settings);

var options = settings.Profiles
    .Where(p => !string.Equals(p, settings.ActualProfile, StringComparison.OrdinalIgnoreCase))
    .ToList();

Cli.Title("FolderProfiler");
Cli.Field("Projeto", settings.ProjectName);
Cli.Field("Perfil atual", settings.ActualProfile);
Cli.Field("Pasta", settings.ProfiledFolderPath);

Cli.Section("Trocar para");

if (options.Count == 0)
{
    Cli.Info("(nenhum outro perfil configurado)");
    Cli.WaitForExit();
    return;
}

for (var i = 0; i < options.Count; i++)
    Cli.Option(i + 1, options[i]);

Cli.Blank();

string? target = null;
while (target is null)
{
    var input = Cli.Prompt("Escolha:");
    if (input is null)
        return;

    if (!int.TryParse(input, out var choice) || choice < 1 || choice > options.Count)
    {
        Cli.Info("Opção inválida.");
        continue;
    }

    target = options[choice - 1];
}

var fromProfile = settings.ActualProfile;

Cli.Blank();
Cli.Section("Trocando perfil");

try
{
    new ProfileSwapper(settingsService).Swap(settings, target);
    Cli.Success($"Perfil trocado com sucesso, de {fromProfile} para {target}.");
}
catch (Exception ex)
{
    Cli.Error(ex.Message);
}

Cli.WaitForExit();
