# Heimdall WinUI Architecture

## Architectural Goal

Heimdall WinUI is designed as a professional, testable, maintainable desktop application.

The UI guides the workflow, but business logic belongs in non-UI layers. The app should remain easy to test without launching the WinUI interface.

## Solution Structure

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

## Project Responsibilities

| Project | Responsibility |
|---|---|
| `Heimdall.App.WinUI` | WinUI pages, shell, navigation, file/folder pickers, UI messages |
| `Heimdall.Application` | Contracts, workflow orchestration, configuration models, session state |
| `Heimdall.BragiCore` | Shared Bragi subject extraction, categorization, and text export logic |
| `Heimdall.Domain` | Core models, value objects, result models, and domain rules |
| `Heimdall.Infrastructure` | CSV reading, subject-list folder reading, matching, HTML rendering, export, run summary |
| `Heimdall.Tests` | xUnit tests for non-UI behavior |

## Dependency Direction

Allowed references:

```text
Heimdall.App.WinUI
  -> Heimdall.Application
  -> Heimdall.Infrastructure
  -> Heimdall.Domain

Heimdall.Infrastructure
  -> Heimdall.Application
  -> Heimdall.Domain
  -> Heimdall.BragiCore

Heimdall.Application
  -> Heimdall.Domain

Heimdall.BragiCore
  -> Heimdall.Domain when needed

Heimdall.Tests
  -> Heimdall.Application
  -> Heimdall.Infrastructure
  -> Heimdall.Domain
  -> Heimdall.BragiCore
```

Avoid:

```text
Domain -> Infrastructure
Domain -> App.WinUI
Application -> Infrastructure
Infrastructure -> App.WinUI
BragiCore -> App.WinUI
```

## Design Principles

- Keep the UI thin.
- Keep domain models independent.
- Keep BragiCore reusable and UI-free.
- Use interfaces for application contracts.
- Use constructor injection.
- Use structured logging.
- Prefer friendly user-facing errors with technical logs.
- Use tests for non-UI logic.
- Preserve existing workflow behavior unless requirements change.

## Main Workflow Services

Important application contracts include:

```text
ICsvBookRecordReader
ICsvSchemaValidator
IBragiSubjectListGenerator
ISubjectListFolderReader
IBookCategoryMatcher
IEmailPreviewBuilder
IHtmlEmailRenderer
IHtmlExportService
IRunSummaryService
IWorkflowOrchestrator
```

## Workflow Orchestrator

`WorkflowOrchestrator` coordinates the complete non-UI workflow:

```text
Load CSV
Generate fresh subject lists
Load existing subject lists
Build preview
Remove book from category preview
Export HTML files
```

The WinUI pages call the workflow. The pages should not duplicate matching, export, or CSV logic.

## Session State

`WizardSessionStore` holds current workflow state across pages:

```text
Selected CSV path
Selected output folder
CSV load result
Subject-list mode
Subject-list folder path
Subject-list load result
Selected categories
Preview result
Removed book selections
Export result
```

## CSV Layer

CSV reading uses named FOLIO columns, not numeric indexes.

Required columns:

```text
instances.title
instances.instance_primary_contributor
instances.notes
instances.subjects
instances.id
```

The CSV layer must validate schema and produce friendly recoverable errors.

## BragiCore Layer

BragiCore owns reusable subject extraction and categorization behavior.

Heimdall uses it for fresh subject-list generation instead of launching an external `Bragi.exe`.

This keeps deployment simpler and improves testability.

## Matching Layer

Book-category matching uses exact normalized subject-heading matching.

Normalization includes:

```text
Trim
Whitespace normalization
Case-insensitive comparison
```

The matcher preserves multi-category behavior.

## HTML Rendering Layer

HTML rendering must:

- Encode category names.
- Encode titles.
- Encode authors.
- Encode summaries.
- Avoid raw CSV injection.
- Exclude category-specific removed books.
- Render CannotSort records safely.

## Export Layer

The export layer writes directly to the selected output folder.

Expected outputs:

```text
CategoryNewBooksyyyy-MM-dd.html
CannotSortBooksyyyy-MM-dd.html
RunSummaryyyyy-MM-dd.txt
```

## Logging

The app uses Serilog through `Microsoft.Extensions.Logging`.

Expected log location:

```text
Documents\Heimdall\Logs
```

Logs should contain technical details. UI messages should stay staff-friendly.

## WinUI Shell

The WinUI shell includes:

```text
Start
Select FOLIO CSV
Choose subject-list source
Select email categories
Preview matched books
Generate HTML emails
```

The sidebar allows returning to unlocked steps while preventing unsafe jumps ahead.

## App Icon and Assets

Phase 19 added:

```text
Assets\Heimdall.ico
Assets\Heimdall.png
Generated Windows package visual assets
```

The app icon is used for app/window identity. It is intentionally not displayed as large content inside workflow pages.
