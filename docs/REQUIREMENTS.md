# Heimdall WinUI Requirements

## Purpose

Heimdall WinUI generates manual HTML email files for new library books by combining official FOLIO CSV records with Bragi subject-list categories.

The app replaces the legacy console-style Heimdall workflow with a modern WinUI 3 guided desktop application.

## Confirmed Decisions

| Area | Final Decision |
|---|---|
| UI type | Windowed desktop app |
| Framework | WinUI 3, C#, .NET 8 |
| Repository | Public GitHub repository |
| Solution file | `Heimdall/Heimdall.slnx` |
| Architecture | App, Application, Domain, Infrastructure, BragiCore, Tests |
| Bragi integration | Reuse Bragi core logic as a shared library |
| Existing Bragi folder | Supported |
| CSV format | Official FOLIO CSV format |
| Supported schema | New FOLIO CSV only |
| Email fields | Title, Author, Summary |
| Category selection | Broad category headings only |
| Output style | Separate HTML file per selected category |
| Multi-match behavior | A book appears in every matching selected category |
| Preview | User previews matched books before export |
| Removal behavior | User can remove individual books from a specific category preview |
| Editable email header | Not supported in current version |
| Uncategorized output | Generate CannotSort HTML file |
| File naming | `CategoryNewBooksDate.html` |
| Output folder | Save directly into selected output folder |
| ThirdIron/API/images | Postponed |
| Email sending | Manual HTML files only |
| Release | Self-contained Windows x64 release after testing |

## MVP Workflow

```text
1. User launches Heimdall.
2. User selects the official FOLIO CSV file.
3. User selects the output folder.
4. User chooses the subject-list source:
   a. Generate fresh Bragi subject lists, or
   b. Use an existing Bragi output folder.
5. App loads available broad categories.
6. User selects broad email categories.
7. App matches CSV books against selected category subject lists.
8. App builds a preview grouped by category.
9. User may remove individual books from specific category previews.
10. App generates one HTML file per selected category.
11. App generates CannotSort output for unmatched or missing-subject records.
12. App generates RunSummary and logs.
13. App saves all files directly into the selected output folder.
14. Finish page shows generated files and allows the user to open the output folder and logs.
```

## Input Requirements

Required CSV columns:

```text
instances.title
instances.instance_primary_contributor
instances.notes
instances.subjects
instances.id
```

Expected behavior:

- Missing required columns produce a friendly user-facing error.
- Empty CSV files produce a friendly user-facing error.
- Locked or unavailable CSV files produce a friendly user-facing error.
- Malformed notes do not crash the app.
- Missing authors or summaries do not crash export.
- Missing or unmatched subjects send the book to CannotSort.

## Subject-List Requirements

Heimdall must support:

```text
Generate fresh Bragi subject lists
Use existing Bragi output folder
```

Expected Bragi subject-list file pattern:

```text
CategorySubjects.txt
```

Examples:

```text
ArtSubjects.txt
BusinessSubjects.txt
ComputerSubjects.txt
EducationSubjects.txt
HistorySubjects.txt
HumanitiesSubjects.txt
SlimSubjects.txt
```

If no valid subject-list files are found, the app must show a friendly error.

If some expected category files are missing, the app may continue with warnings if at least one usable category exists.

## Category Requirements

The current version supports broad category headings only.

Individual subject-heading selection is out of scope.

Known broad categories include:

```text
Art
Biology
Business
Chemistry
Computer
Education
Fiction
Forensics
Geoscience
History
HPER
Humanities
IDT
InterDis
Math
Music
Nursing
Performance
Physics
Politics
Psych
SLIM
```

## Matching Requirements

Matching must:

- Split `instances.subjects` by semicolon.
- Trim values.
- Normalize whitespace.
- Compare case-insensitively.
- Use exact subject-heading matching.
- Preserve multi-category matching.
- Avoid first-match-wins logic.
- Avoid duplicate book entries inside the same category.
- Send missing-subject and unmatched records to CannotSort.

## Preview Requirements

The preview must show:

- Matched books grouped by category.
- Category counts.
- CannotSort count.
- Book title.
- Author.
- Summary preview.
- Category-specific remove action.

Removal behavior:

```text
Removing a book from Art must not remove it from History.
Removing a book from one category affects only that category's final HTML output.
```

## Export Requirements

Export must generate:

```text
One HTML file per selected category
CannotSortBooksyyyy-MM-dd.html when CannotSort records exist
RunSummaryyyyy-MM-dd.txt
Logs
```

Export must:

- Save directly into the selected output folder.
- HTML-encode all user/CSV content.
- Use readable fallback text for missing author or summary.
- Preserve category-specific removals.
- Include run summary context.

## Logging Requirements

Logs should include technical details for troubleshooting while user-facing messages remain friendly.

Expected log location:

```text
Documents\Heimdall\Logs
```

## Out of Scope

The following are intentionally postponed:

- Direct email sending.
- Outlook automation.
- SMTP integration.
- Microsoft Graph integration.
- ThirdIron integration.
- API enrichment.
- Book cover images.
- Editable email headers.
- Individual subject-heading selection.
- Legacy CSV schemas.
- Automatic scheduling.
