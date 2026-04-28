# Heimdall WinUI

Heimdall WinUI is a modern Windows desktop application for generating manual HTML email files from official FOLIO CSV book records and Bragi-generated subject category lists.

The application is being rebuilt from the legacy Heimdall console utility into a professional WinUI 3 desktop application with clean architecture, dependency injection, logging, testable core logic, and a self-contained Windows release path.

## Current Status

Development status: planning and solution bootstrap.

This repository is being built phase by phase. The first implementation milestone is the project skeleton, documentation, and architectural foundation.

## MVP Scope

The Phase 1 MVP will support:

- Loading the official FOLIO CSV format.
- Reading book title, author, notes, subjects, and record ID from named CSV columns.
- Generating fresh Bragi subject lists through reusable Bragi core logic.
- Using an existing Bragi output folder.
- Selecting broad subject categories only.
- Matching books against selected category subject lists.
- Allowing books to appear in every matching category.
- Previewing matched books before export.
- Removing individual books from a specific category preview.
- Generating one HTML file per selected category.
- Generating `CannotSortBooks.html` for unmatched records.
- Generating a run summary and logs.
- Saving all generated files directly into the selected output folder.

## Out of Scope for Phase 1

The following are intentionally postponed:

- ThirdIron integration.
- API-based enrichment.
- Book cover images.
- Direct email sending.
- Outlook, SMTP, or Microsoft Graph automation.
- Editable email headers.
- Individual subject-heading selection.

## Input CSV Format

The application targets the new official FOLIO CSV schema.

Required columns:

```text
instances.title
instances.instance_primary_contributor
instances.notes
instances.subjects
instances.id
