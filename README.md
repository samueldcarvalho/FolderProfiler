# FolderProfiler

A simple CLI tool to switch between folder profiles using `.zip` files, with timestamped backups.

## Requirements

- .NET 10 SDK
- Windows

## Configuration

On the first run, the app creates:

`C:\FolderProfiler\settings.json`

Edit the file and run the app again. Example:

```json
{
  "projectName": "MyGame",
  "profiledFolderPath": "D:\\Games\\MyGame\\profile",
  "profiles": [
    "me",
    "wife",
    "friend"
  ],
  "actualProfile": "me"
}
```

Windows paths using single backslashes (e.g., `C:\Users\...`) are supported — the app normalizes them before reading. Double backslashes (`\\`) and forward slashes (`/`) also work.

## File Structure

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

- **Active profile**: folder located at `profiledFolderPath`.
- **Inactive profiles**: stored as `.zip` files in `Files/<profile>/`.
- When activating a profile, the zip file used for extraction is moved to `backup/yyyyMMdd_HHmmss.zip`.

## Bootstrap

1. Fill in `settings.json` with the current folder path and its corresponding `actualProfile`.
2. Run the app — it automatically creates the `Files\<profile>\backup\` directory structure.
3. Place the base `.zip` file for each profile (except the currently active one) in:

   `C:\FolderProfiler\<projectName>\Files\<profile>\<folderName>_<profile>.zip`

The zip file must contain the profiled folder at its root (e.g., `.agents\...`), not loose files or an extra nested `.agents\.agents\...` directory.

## Run

```bash
dotnet run
```

## Publish as an Executable

The repository includes a `nuget.config` pointing exclusively to nuget.org to avoid corporate package feeds. Publishing does not generate `.pdb` files.

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish
```

Output: `publish\FolderProfiler.exe`
