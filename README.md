# Heimdall WinUI
[![ci](https://github.com/sarosh-jawed/Heimdall-WinUI/actions/workflows/ci.yml/badge.svg)](https://github.com/sarosh-jawed/Heimdall-WinUI/actions/workflows/ci.yml)

Heimdall WinUI is a professional Windows desktop application for generating manual HTML new-book email files from official FOLIO CSV book records and Bragi subject-list categories.

The app modernizes the original Heimdall workflow into a guided WinUI 3 desktop utility with clean architecture, dependency injection, structured logging, testable non-UI logic, preview/review behavior, and a self-contained Windows release path.

## Current Status

Development status: Phase 22 complete. Preparing Phase 23 self-contained Windows x64 release.

Completed implementation through Phase 22 includes:

- Official FOLIO CSV loading.
- Bragi subject-list generation through shared core logic.
- Existing Bragi output folder mode.
- Broad category selection.
- Exact normalized subject-heading matching.
- Multi-category book matching.
- Preview grouped by category.
- Category-specific book removal.
- HTML email rendering.
- CannotSort output.
- RunSummary generation.
- Friendly error handling.
- Automated regression tests.
- Professional WinUI shell polish and app icon assets.
- GitHub Actions CI for restore, Release build, regression tests, and full test suite.
- End-to-end manual testing for fresh Bragi generation and existing Bragi folder workflows.
- Release screenshot documentation.

## What Heimdall Does

Heimdall helps library staff create manual email-ready HTML files for new books.

The app combines:

1. An official FOLIO CSV file containing book records.
2. Bragi-generated subject-list text files.
3. User-selected broad email categories.

It then creates:

- One HTML file per selected category.
- A CannotSort HTML file for unmatched or missing-subject records.
- A RunSummary text file.
- Technical logs for troubleshooting.

Heimdall does not send emails automatically. Staff can review the generated files and use them manually.

## Confirmed Workflow

```text
1. Open Heimdall.
2. Select the official FOLIO CSV file.
3. Choose the output folder.
4. Choose the subject-list source:
   a. Generate fresh Bragi subject lists, or
   b. Use an existing Bragi output folder.
5. Select broad email categories.
6. Preview matched books by category.
7. Remove books from individual category previews if needed.
8. Generate HTML emails.
9. Review generated files, CannotSort output, RunSummary, and logs.
```

## Input CSV Format

Heimdall supports the official FOLIO CSV schema used for the new-book workflow.

Required columns:

```text
instances.title
instances.instance_primary_contributor
instances.notes
instances.subjects
instances.id
```

Optional fields may be preserved internally for diagnostics and future expansion, but the email output currently uses:

```text
Title      -> instances.title
Author     -> instances.instance_primary_contributor
Summary    -> extracted from instances.notes
Subjects   -> instances.subjects
Record ID  -> instances.id
```

## Subject-List Sources

Heimdall supports two modes.

### Generate fresh Bragi subject lists

The app runs shared Bragi core logic inside Heimdall. It generates category subject files from the selected FOLIO CSV.

### Use existing Bragi output folder

The user can select a folder that already contains Bragi subject-list files, such as:

```text
ArtSubjects.txt
BusinessSubjects.txt
ComputerSubjects.txt
EducationSubjects.txt
HistorySubjects.txt
HumanitiesSubjects.txt
SlimSubjects.txt
```

## Matching Rules

Heimdall matches books to selected categories using exact normalized subject-heading matching.

Rules:

- Split `instances.subjects` by semicolon.
- Trim each subject value.
- Normalize whitespace.
- Compare case-insensitively.
- Match exact subject headings against selected category subject lists.
- Preserve multi-category matches.
- Send missing-subject or unmatched records to CannotSort.

Heimdall does not use first-match-wins behavior. If a book matches multiple selected categories, it remains in each matched category.

## Output Files

Generated files are saved directly into the selected output folder.

Heimdall intentionally generates category HTML files only for selected categories that contain matched books. Empty selected categories are skipped to avoid creating blank email files.

Examples:

```text
ArtNewBooks2026-05-01.html
EducationNewBooks2026-05-01.html
HistoryNewBooks2026-05-01.html
CannotSortBooks2026-05-01.html
RunSummary2026-05-01.txt
```

The exact date uses the configured output date format.

## Project Structure

```text
Heimdall-WinUI/
├── Heimdall/
│   ├── Heimdall.slnx
│   ├── Heimdall.App.WinUI/
│   ├── Heimdall.Application/
│   ├── Heimdall.BragiCore/
│   ├── Heimdall.Domain/
│   ├── Heimdall.Infrastructure/
│   └── Heimdall.Tests/
├── docs/
├── README.md
├── CHANGELOG.md
├── LICENSE.txt
├── .editorconfig
└── .gitignore
```

## Build Instructions

From the repository root:

```powershell
dotnet restore .\Heimdall\Heimdall.slnx
dotnet build .\Heimdall\Heimdall.slnx -c Debug
```

To run locally in Visual Studio:

```text
Open: Heimdall\Heimdall.slnx
Configuration: Debug
Platform: x64
Startup project: Heimdall.App.WinUI (Package)
Press F5
```

## Test Instructions

Run regression tests:

```powershell
dotnet test .\Heimdall\Heimdall.slnx -c Debug --filter FullyQualifiedName~Regression
```

Run the full test suite:

```powershell
dotnet test .\Heimdall\Heimdall.slnx -c Debug
```

## Release Instructions

The intended release path is a self-contained Windows x64 artifact.

Release preparation is handled in a later phase. Before release:

```powershell
dotnet clean .\Heimdall\Heimdall.slnx
dotnet restore .\Heimdall\Heimdall.slnx
dotnet build .\Heimdall\Heimdall.slnx -c Release
dotnet test .\Heimdall\Heimdall.slnx -c Release
```

The final release artifact should be named similar to:

```text
Heimdall-v1.0.0-win-x64.zip
```

Users should extract the full ZIP before running the app.

## Current Limitations

The following are intentionally out of scope for the current version:

- Direct email sending.
- Outlook, SMTP, or Microsoft Graph integration.
- ThirdIron integration.
- API-based enrichment.
- Book cover images.
- Editable email headers.
- Individual subject-heading selection.
- Legacy CSV schemas.
- Automatic email scheduling.

## Future Work

Possible future enhancements:

- Self-contained release automation.
- GitHub Actions CI.
- Additional screenshot documentation.
- User guide with real workflow examples.
- Optional book image or metadata enrichment.
- Optional direct email integration after staff review and approval.
- Installer or MSIX distribution option.
- Accessibility review.

## Documentation

See:

```text
docs/REQUIREMENTS.md
docs/ARCHITECTURE.md
docs/TEST-CASES.md
docs/RELEASE.md
docs/SCREENSHOTS.md
```
