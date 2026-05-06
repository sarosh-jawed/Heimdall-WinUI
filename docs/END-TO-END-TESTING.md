# Heimdall WinUI End-to-End Manual Testing

## Purpose

This document records the Phase 22 end-to-end manual testing for Heimdall WinUI.

The goal is to confirm that both supported workflows work from beginning to end without developer intervention:

1. Fresh Bragi subject-list generation.
2. Existing Bragi output folder mode.

## Test Environment

| Item | Value |
|---|---|
| App | Heimdall WinUI |
| Phase | 22 |
| Configuration | Debug |
| Platform | x64 |
| Startup project | Heimdall.App.WinUI (Package) |
| Solution | `Heimdall/Heimdall.slnx` |
| Test CSV | `2nd Floor Display Books.csv` |
| Test date | 2026-05-05 |
| Tester | Sarosh Jawed |

## Test CSV Summary

The test CSV contains:

```text
Total records: 34
Required columns: present
Blank titles: 0
Blank IDs: 0
Blank authors: 13
Blank subjects: 4
```

Required columns verified:

```text
instances.title
instances.instance_primary_contributor
instances.notes
instances.subjects
instances.id
```

## Evidence Handling

The Phase 22 review evidence included generated output folders for both workflows:

```text
ScenarioA-FreshBragi-Output
ScenarioB-ExistingBragi-Output
```

The evidence included category HTML files, CannotSortBooks HTML files, RunSummary text files, and generated Bragi subject-list files. The original input CSV and local log files were not committed to the repository.

Logs were verified locally from:

```text
Documents\Heimdall\Logs
```

The release evidence documents that logs were created and reviewed without storing local machine-specific or sensitive log files in Git.

## Scenario A - Fresh Bragi Generation

### Workflow

```text
1. Opened Heimdall.
2. Selected 2nd Floor Display Books.csv.
3. Selected ScenarioA-FreshBragi-Output folder.
4. Chose Generate fresh Bragi subject lists.
5. Selected broad email categories.
6. Built preview.
7. Removed one book from the History category.
8. Exported.
9. Opened output folder.
10. Verified category HTML files.
11. Verified CannotSortBooks output.
12. Verified RunSummary.
13. Verified logs.
```

### Result

Status:

```text
PASS
```

Observed outputs:

```text
Category HTML files generated: 8
Total generated files: 10
CannotSort file generated: YES
RunSummary generated: YES
Logs generated: YES
Total records read in RunSummary: 34
CannotSort count in RunSummary: 4
Subject-list mode: GenerateFresh
Subject-list folder: ScenarioA-FreshBragi-Output\BragiSubjectLists
Removed category: History
Removed book title: A history of women in the West / Georges Duby and Michelle Perrot, general editors.
```

Empty selected categories:

```text
Selected categories with no matched books did not generate empty HTML files. This is expected behavior because Heimdall only creates category email files when there are matched active books for that category.
```

Category output files generated:

```text
ArtNewBooks2026-05-05.html
EducationNewBooks2026-05-05.html
FictionNewBooks2026-05-05.html
HistoryNewBooks2026-05-05.html
HumanitiesNewBooks2026-05-05.html
PoliticsNewBooks2026-05-05.html
PsychNewBooks2026-05-05.html
SLIMNewBooks2026-05-05.html
CannotSortBooks2026-05-05.html
RunSummary2026-05-05.txt
```

Notes:

```text
Fresh Bragi generation completed successfully. The app generated BragiSubjectLists, loaded 22 broad categories, built a preview with 8 matched output categories, and exported 10 files. CannotSort count was 4, matching the expected blank/unmatched subject records. One History book was removed from the History category only, reducing History from 20 matched books to 19 active books. No warnings were reported.
```

## Scenario B - Existing Bragi Folder

### Workflow

```text
1. Opened Heimdall.
2. Selected the same 2nd Floor Display Books.csv.
3. Selected ScenarioB-ExistingBragi-Output folder.
4. Chose Use an existing Bragi output folder.
5. Selected the Bragi subject-list folder generated during Scenario A.
6. Selected broad email categories.
7. Built preview.
8. Exported.
9. Opened output folder.
10. Verified category HTML files.
11. Verified CannotSortBooks output.
12. Verified RunSummary.
13. Verified logs.
```

### Result

Status:

```text
PASS
```

Observed outputs:

```text
Category HTML files generated: 8
Total generated files: 10
CannotSort file generated: YES
RunSummary generated: YES
Logs generated: YES
Total records read in RunSummary: 34
CannotSort count in RunSummary: 4
Subject-list mode: ExistingFolder
Existing Bragi folder used: C:\Users\sjawed\Desktop\Heimdall Phase 22 Manual Test\ScenarioA-FreshBragi-Output\BragiSubjectLists
```

Empty selected categories:

```text
Selected categories with no matched books did not generate empty HTML files. This is expected behavior because Heimdall only creates category email files when there are matched active books for that category.
```

Category output files generated:

```text
ArtNewBooks2026-05-05.html
EducationNewBooks2026-05-05.html
FictionNewBooks2026-05-05.html
HistoryNewBooks2026-05-05.html
HumanitiesNewBooks2026-05-05.html
PoliticsNewBooks2026-05-05.html
PsychNewBooks2026-05-05.html
SLIMNewBooks2026-05-05.html
CannotSortBooks2026-05-05.html
RunSummary2026-05-05.txt
```

Notes:

```text
Existing Bragi folder mode completed successfully. Heimdall read the BragiSubjectLists folder generated in Scenario A, loaded the same broad categories, built the preview, and exported the expected HTML files. Scenario B did not remove any books, so History remained at 20 active books. CannotSort count remained 4. No warnings were reported.
```

## Manual Verification Checklist

```text
[x] App can be used without reading code.
[x] FOLIO CSV loads successfully.
[x] Fresh Bragi generation works.
[x] Existing Bragi folder mode works.
[x] Category selection works.
[x] Preview builds correctly.
[x] Category-specific removal works.
[x] Export completes successfully.
[x] HTML files open correctly.
[x] CannotSort output is generated when records are unmatched.
[x] RunSummary contains source CSV, subject-list mode, counts, generated files, and timing.
[x] Logs are written.
[x] No raw exception text appears in normal user-facing flows.
```

## Final Phase 22 Decision

```text
PASS
```

Final notes:

```text
Phase 22 passed. Both supported workflows were tested end to end using the same official FOLIO CSV. Scenario A verified fresh Bragi subject-list generation, preview, category-specific removal, export, CannotSort output, RunSummary, and logs. Scenario B verified existing Bragi folder mode using the Scenario A BragiSubjectLists folder. Both workflows completed without developer intervention, with no warnings and no raw exception text shown to the user.
```
