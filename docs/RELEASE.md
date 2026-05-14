# Heimdall WinUI Release Plan

## Release Goal

Heimdall is released as a self-contained Windows x64 desktop app.

Preferred user experience:

```text
1. User downloads Heimdall-v1.0.0-win-x64.zip.
2. User extracts the full ZIP.
3. User runs Heimdall.exe.
4. App launches without requiring Visual Studio.
```

## Current Release Status

Release preparation is planned for a later phase.

The current repository is not yet a final release package. Phase 20 only documents the intended release path.

## Pre-Release Checklist

Before creating a release:

```text
[ ] main branch is clean.
[ ] README is updated.
[ ] CHANGELOG is updated.
[ ] Version is selected.
[ ] Debug build passes.
[ ] Release build passes.
[ ] Regression tests pass.
[ ] Full test suite passes.
[ ] Manual fresh Bragi workflow passes.
[ ] Manual existing Bragi folder workflow passes.
[ ] Output files are verified.
[ ] RunSummary is verified.
[ ] Logs are verified.
[ ] Known limitations are documented.
```

## Standard Verification Commands

From the repository root:

```powershell
dotnet clean .\Heimdall\Heimdall.slnx
dotnet restore .\Heimdall\Heimdall.slnx
dotnet build .\Heimdall\Heimdall.slnx -c Debug
dotnet test .\Heimdall\Heimdall.slnx -c Debug --filter FullyQualifiedName~Regression
dotnet test .\Heimdall\Heimdall.slnx -c Debug
```

For release configuration:

```powershell
dotnet clean .\Heimdall\Heimdall.slnx
dotnet restore .\Heimdall\Heimdall.slnx
dotnet build .\Heimdall\Heimdall.slnx -c Release
dotnet test .\Heimdall\Heimdall.slnx -c Release
```

## Planned Artifact Name

Use a versioned ZIP name:

```text
Heimdall-v1.0.0-win-x64.zip
```

For beta testing, use:

```text
Heimdall-v1.0.0-beta.1-win-x64.zip
```

## Publish Notes

The app is a WinUI 3 packaged desktop project. The final publish command/profile should be verified during the release phase because WinUI packaging and self-contained distribution can vary depending on the selected packaging path.

A likely command shape is:

```powershell
dotnet publish .\Heimdall\Heimdall.App.WinUI\Heimdall.App.WinUI.csproj -c Release -r win-x64 --self-contained true
```

Do not finalize a GitHub release until the published artifact has been extracted and tested on a clean Windows environment.

## User Instructions for ZIP Release

Include these instructions in the GitHub release notes:

```text
1. Download Heimdall-v1.0.0-win-x64.zip.
2. Right-click the ZIP and choose Extract All.
3. Open the extracted folder.
4. Run Heimdall.exe.
5. Do not run the app from inside the ZIP.
6. Do not move only Heimdall.exe by itself. Keep the extracted folder together.
```

## Known Release Limitations

Current version does not include:

```text
Direct email sending
Outlook integration
SMTP integration
Microsoft Graph integration
ThirdIron integration
Book cover images
API enrichment
Editable email headers
Legacy CSV schemas
```

## Manual Release Smoke Test

After publishing:

```text
[ ] Extract ZIP into a new folder.
[ ] Run Heimdall.exe.
[ ] Confirm app icon appears.
[ ] Select official FOLIO CSV.
[ ] Select output folder.
[ ] Test fresh Bragi generation.
[ ] Test existing Bragi folder mode.
[ ] Select categories.
[ ] Preview matched books.
[ ] Remove one book from one category.
[ ] Export.
[ ] Open output folder.
[ ] Verify HTML files.
[ ] Verify CannotSort output.
[ ] Verify RunSummary.
[ ] Verify logs.
```
