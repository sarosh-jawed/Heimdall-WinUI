# Heimdall WinUI Requirements

## Purpose

Heimdall WinUI will generate manual HTML email files for new library books by combining official FOLIO CSV records with Bragi-generated subject category lists.

The new application replaces the legacy Heimdall console workflow with a modern WinUI 3 desktop workflow.

## Confirmed Decisions

| Area | Final Decision |
|---|---|
| UI type | Windowed desktop app |
| Framework | WinUI 3, C#, .NET |
| Repository | New public GitHub repository |
| Bragi integration | Reuse Bragi core logic as a shared library |
| Existing Bragi folder | User may use an existing Bragi output folder |
| CSV format | Official FOLIO CSV format |
| Supported schema | New FOLIO CSV only |
| Email fields | Title, Author, Summary |
| Category selection | Broad category headings only |
| Output style | Separate HTML file per selected category |
| Multi-match behavior | A book appears in every matching category |
| Preview | User previews matched books before export |
| Removal behavior | User can remove individual books from a category preview |
| Editable email header | Not supported in Phase 1 |
| Uncategorized output | Generate `CannotSortBooks.html` |
| File naming | `CategoryNewBooksDate.html` |
| Output folder | Save directly into selected output folder |
| ThirdIron/API/images | Postponed |
| Email sending | Manual HTML files only |
| Release | Self-contained Windows x64 release after testing |

## MVP Workflow

```text
1. User launches Heimdall WinUI.
2. User selects the official FOLIO CSV file.
3. User selects the output folder.
4. User chooses the subject-list source:
   a. Generate fresh Bragi subject lists, or
   b. Use an existing Bragi output folder.
5. App loads available broad categories.
6. User selects broad categories.
7. App matches CSV books against selected category subject lists.
8. App builds a preview grouped by category.
9. User may remove individual books from specific category previews.
10. App generates one HTML file per selected category.
11. App generates CannotSortBooks.html for unmatched records.
12. App generates RunSummary and logs.
13. App saves all files directly into the selected output folder.
14. Finish page shows generated files and allows the user to open the output folder.