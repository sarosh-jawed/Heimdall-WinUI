# Heimdall v1.0.0 Release Notes

## Artifact

```text
Heimdall-v1.0.0-win-x64.zip
```

## Overview

Heimdall WinUI is a Windows desktop application that generates manual HTML new-book email files from official FOLIO CSV records and Bragi subject-list categories.

This release provides the first complete production-ready workflow for library staff to load a FOLIO CSV, prepare or reuse Bragi subject lists, select broad email categories, preview matched books, remove category-specific records when needed, and export reviewable HTML email files.

## Included in v1.0.0

- Guided WinUI 3 wizard workflow.
- Official FOLIO CSV loading.
- Fresh Bragi subject-list generation.
- Existing Bragi output folder mode.
- Broad category selection.
- Exact normalized subject-heading matching.
- Multi-category book matching.
- Preview grouped by category.
- Category-specific book removal.
- HTML email generation.
- CannotSortBooks output.
- RunSummary output.
- Technical logs.
- Friendly user-facing errors.
- App icon and release UI polish.
- GitHub Actions CI.
- End-to-end manual testing documentation.
- Release screenshots and workflow diagrams.

## User Instructions

```text
1. Download Heimdall-v1.0.0-win-x64.zip.
2. Right-click the ZIP and choose Extract All.
3. Open the extracted folder.
4. Run Heimdall.exe.
5. Do not run the app from inside the ZIP.
6. Do not move only Heimdall.exe by itself. Keep the extracted folder together.
```

## Verified Workflows

The following workflows were verified before release:

```text
Fresh Bragi generation workflow
Existing Bragi output folder workflow
```

Both workflows successfully generated:

```text
Category HTML files
CannotSortBooks HTML file
RunSummary text file
Logs
```

## Important Behavior Notes

- The app name appears as Heimdall.
- The release executable is `Heimdall.exe`.
- Heimdall generates category HTML files only for selected categories that contain matched active books.
- Empty selected categories are skipped to avoid blank email files.
- A book can appear in multiple category outputs when it matches multiple selected categories.
- Removing a book from one category preview does not remove it from other matched categories.
- CannotSortBooks is generated when records are unmatched or have missing subjects.
- Heimdall does not send emails automatically. It generates files for staff review and manual use.

## Known Limitations

This release does not include:

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
Automatic scheduling
```
