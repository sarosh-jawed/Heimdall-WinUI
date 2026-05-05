# Heimdall WinUI Test Cases

## Test Strategy

Heimdall uses automated tests for non-UI logic and manual tests for WinUI interaction.

The most important rule is that workflow behavior should be testable without launching the desktop UI.

## Standard Verification Commands

From the repository root:

```powershell
dotnet clean .\Heimdall\Heimdall.slnx
dotnet restore .\Heimdall\Heimdall.slnx
dotnet build .\Heimdall\Heimdall.slnx -c Debug
dotnet test .\Heimdall\Heimdall.slnx -c Debug --filter FullyQualifiedName~Regression
dotnet test .\Heimdall\Heimdall.slnx -c Debug
```

Expected result:

```text
Build succeeded
Regression tests passed
Full test suite passed
```

## Automated Test Areas

Current test coverage includes:

```text
CSV loading
CSV schema validation
Summary extraction
BragiCore subject extraction
BragiCore categorization
Fresh subject-list generation
Existing subject-list folder mode
Category file detection
Book-to-category matching
Multi-category matching
CannotSort handling
Email preview building
Category-specific removal
HTML rendering
HTML encoding
HTML export
RunSummary generation
Workflow orchestration
Regression coverage
```

## Regression Test Scope

Regression tests should continue to cover:

```text
Official CSV loading
Required schema validation
Bragi fresh generation
Existing subject-list folder mode
Multi-category matching
Preview removal
HTML rendering
CannotSort output
RunSummary export
```

Run:

```powershell
dotnet test .\Heimdall\Heimdall.slnx -c Debug --filter FullyQualifiedName~Regression
```

## Manual UI Test Checklist

Use Visual Studio:

```text
Open: Heimdall\Heimdall.slnx
Configuration: Debug
Platform: x64
Startup project: Heimdall.App.WinUI (Package)
Press F5
```

Checklist:

```text
[ ] App launches without crashing.
[ ] Window title shows Heimdall.
[ ] App icon appears in the window/taskbar.
[ ] Sidebar text is readable.
[ ] Future sidebar steps are visible but not freely jumpable before workflow progress.
[ ] Back button works.
[ ] Main action button validates the current step.
[ ] Selecting no CSV gives a friendly message.
[ ] Selecting no output folder gives a friendly message.
[ ] Selecting wrong CSV gives a friendly message.
[ ] Selecting official FOLIO CSV succeeds.
[ ] Existing Bragi folder mode works.
[ ] Fresh Bragi generation mode works.
[ ] Category selection loads broad categories.
[ ] Select all and Clear all work.
[ ] Continuing with zero categories gives a friendly message.
[ ] Preview shows matched books grouped by category.
[ ] CannotSort count appears.
[ ] Removing a book from one category does not remove it from another category.
[ ] Export generates HTML files.
[ ] Export generates CannotSort output when needed.
[ ] Export generates RunSummary.
[ ] Open output folder works.
[ ] Open logs works.
[ ] No raw exception text appears in normal user-facing flows.
[ ] Technical details are still written to logs.
```

## Negative Test Cases

### CSV missing required column

Expected:

```text
Friendly user message
Technical log details
No app crash
```

### Empty CSV

Expected:

```text
Friendly user message
No preview or export until corrected
```

### Wrong file type

Expected:

```text
Friendly user message
No workflow state corruption
```

### Empty existing Bragi folder

Expected:

```text
Friendly user message saying no subject-list files were found
```

### Missing some Bragi category files

Expected:

```text
Warning if at least one category exists
User may continue with available categories
```

### Output folder unavailable

Expected:

```text
Friendly export error
Technical log details
User can pick another folder
```

## Manual Documentation Test Checklist

For Phase 20:

```text
[ ] README explains the app for a staff user.
[ ] README explains build and test commands for a developer.
[ ] REQUIREMENTS.md reflects the completed MVP behavior.
[ ] ARCHITECTURE.md matches the actual project structure.
[ ] TEST-CASES.md includes current automated and manual testing.
[ ] RELEASE.md explains the intended release process.
[ ] SCREENSHOTS.md exists and explains where screenshots belong.
[ ] Build/test commands in docs work when copied.
[ ] Release instructions do not claim a completed release before Phase 23.
```
