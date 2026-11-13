# FolderProfiler

CLI simples para trocar pastas de perfil via `.zip`, com backup timestampado.

## Requisitos

- .NET 10 SDK
- Windows

## Configuração

Na primeira execução o app cria:

`C:\FolderProfiler\settings.json`

Edite o arquivo e rode de novo. Exemplo:

```json
{
  "projectName": "MeuJogo",
  "profiledFolderPath": "D:\Jogos\MeuJogo\profile",
  "profiles": [ "eu", "mulher", "amigo" ],
  "actualProfile": "eu"
}
```

Paths Windows com `\` simples (tipo `C:\Users\...`) são aceitos — o app normaliza antes de ler. `\\` ou `/` também funcionam.

## Layout dos arquivos

```text
C:\FolderProfiler/
  settings.json
  <projectName>/
    Files/
      <profile>/
        <folderName>_<profile>.zip
        backup/
          20261113_133045.zip
```

- Perfil **ativo**: pasta em `profiledFolderPath`
- Perfis **inativos**: zip em `Files/<profile>/`
- Ao ativar um perfil: o zip usado na extração vai para `backup/yyyyMMdd_HHmmss.zip`

## Bootstrap

1. Preencha o `settings.json` com a pasta atual e `actualProfile` correspondente.
2. Rode o app — ele cria a árvore `Files\<perfil>\backup\` automaticamente.
3. Coloque o zip base de cada perfil (exceto o atual) em:

`C:\FolderProfiler\<projectName>\Files\<perfil>\<folderName>_<perfil>.zip`

O zip deve ter a pasta perfilada na raíz (ex.: `.agents\...`), não os arquivos soltos nem `.agents\.agents\...`.

## Rodar

```bash
dotnet run
```

## Publicar .exe

O repo inclui `nuget.config` apontando só para nuget.org (evita feeds corporativos). Publish não gera `.pdb`.

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

Saída: `publish\FolderProfiler.exe`
