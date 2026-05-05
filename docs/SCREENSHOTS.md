# Heimdall WinUI Screenshots

## Purpose

This document tracks screenshots that should be added before release.

Screenshots help staff users understand the workflow and help reviewers quickly see the app's current UI state.

## Recommended Screenshot Folder

Use:

```text
docs/screenshots/
```

Do not commit screenshots from local test data if they expose sensitive, private, or staff-only information.

## Required Screenshots Before Release

Recommended screenshot set:

```text
01-start-page.png
02-select-folio-csv.png
03-choose-subject-list-source.png
04-select-email-categories.png
05-preview-matched-books.png
06-generate-html-emails-before-export.png
07-export-complete.png
08-output-folder-example.png
09-high-level-workflow.png
10-classification-logic.png
```

## Screenshot Checklist

```text
[x] Start page shows the workflow overview.
[x] Select FOLIO CSV page shows CSV and output-folder controls.
[x] Choose subject-list source page shows both Bragi options.
[x] Select email categories page shows category counts.
[x] Preview matched books page shows grouped category preview.
[x] Generate HTML emails page shows pre-export state.
[x] Export complete page shows success, RunSummary, category counts, and generated files.
[x] Output folder screenshot shows generated HTML files and RunSummary.
[x] High-level workflow diagram shows the complete app workflow.
[x] Classification logic diagram shows exact matching, multi-category behavior, and CannotSort behavior.
```

## Screenshot Guidelines

Use screenshots that are:

```text
Readable
Not cropped too tightly
Free of private information
Consistent in window size
Captured after Phase 19 UI polish
```

Preferred Visual Studio run settings when capturing app screenshots:

```text
Configuration: Debug
Platform: x64
Startup project: Heimdall.App.WinUI (Package)
```

The small XAML debug toolbar may appear during F5 debugging. It does not appear in release builds. Hide it before final screenshots if needed:

```text
Visual Studio
Tools
Options
Debugging
General
Uncheck: Enable UI Debugging Tools for XAML
```

## Current Screenshot Status

The Phase 22/Phase 23 preparation screenshot set has been added under:

```text
docs/screenshots/
```

Included screenshots:

```text
01-start-page.png
02-select-folio-csv.png
03-choose-subject-list-source.png
04-select-email-categories.png
05-preview-matched-books.png
06-generate-html-emails-before-export.png
07-export-complete.png
08-output-folder-example.png
09-high-level-workflow.png
10-classification-logic.png
```
