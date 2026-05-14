# Changelog

All notable changes to Heimdall WinUI will be documented in this file.

## [Unreleased]

No unreleased changes.

## [1.0.0] - 2026-05-05

### Added

- Added Phase 23 final release preparation for `v1.0.0`.
- Added self-contained Windows x64 release notes for `v1.0.0`.
- Added release-prep documentation cleanup for Phase 23 readiness.
- Added Phase 22 end-to-end manual testing documentation for fresh Bragi generation and existing Bragi folder workflows.
- Added GitHub Actions CI workflow for restore, Release build, regression tests, and full test suite on `windows-latest`.
- Added CI status badge to README.
- Updated Phase 20 documentation across README and docs.
- Rewrote README to reflect completed functionality instead of early planning status.
- Updated requirements, architecture, test cases, and release documentation.
- Added `docs/SCREENSHOTS.md` to guide screenshot capture before release.
- Added release screenshots under `docs/screenshots/`.
- Added workflow and classification diagrams for release documentation.
- Added completion summary content on the Export & Finish page.
- Added output folder, generated files, category counts, CannotSort count, and run timing display after export.
- Added Open Logs button.
- Added full RunSummary context including source CSV path, subject-list mode, subject-list folder path, selected categories, total records read, matched counts, removed counts, CannotSort count, generated files, run start/end timestamp, and warnings.
- Added RunSummary service tests and context-based export summary coverage.
- Connected the Export & Finish page to the real export workflow.
- Added final export action from the WinUI wizard.
- Added generated file list display after export.
- Added Open Output Folder behavior.
- Added focused export service tests for file naming, direct output-folder writes, CannotSort generation, RunSummary generation, and expected overwrite behavior.
- Added table-based HTML email rendering.
- Added HTML encoding for category names, titles, authors, and summaries.
- Added readable fallback text for empty author and summary fields.
- Added renderer tests for HTML safety, empty fields, removed-book exclusion, and CannotSort rendering.
- Added export-level coverage for encoded HTML output.
- Added functional preview and remove-record behavior.
- Displayed matched books grouped by selected category.
- Added book cards with title, author, summary preview, and category-specific remove action.
- Added CannotSort preview summary.
- Preserved multi-category matching behavior when removing a book from one category.
- Added tests confirming removed books do not appear in that category export while remaining in other matched categories.
- Added functional category selection page.
- Loaded broad categories from generated or existing Bragi subject lists.
- Added subject counts per category.
- Added Select all and Clear all controls.
- Added category selection validation before preview.
- Connected selected categories to workflow preview generation.
- Added wizard step validation so users cannot continue before required workflow state is ready.
- Added WinUI file and folder picker support.
- Added CSV picker restricted to `.csv` files.
- Added output folder picker.
- Added existing Bragi subject-list folder picker.
- Persisted selected paths in the wizard session store.
- Added friendly picker cancellation and validation messages in the UI.
- Added basic WinUI wizard shell and navigation.
- Added step rail, progress indicator, Back and Next buttons, busy state, and friendly status area.
- Added placeholder wizard pages for Start, Load Input, Subject Source, Category Selection, Preview Books, and Export Finish.
- Prepared the UI structure for file picker, category loading, preview, and export integration.
- Added complete non-UI workflow orchestration.
- Expanded wizard session state to store CSV results, subject-list results, preview results, and removed book selections.
- Added email preview building from category match results.
- Added HTML rendering for category and CannotSort email files.
- Added HTML export service and run summary text generation.
- Added workflow tests for CSV loading, fresh Bragi generation, existing folder loading, preview building, removal behavior, export, cancellation, and error logging.
- Added book-to-category matching for selected broad categories.
- Added exact normalized subject-heading matching.
- Added multi-category book matching without first-match-wins behavior.
- Added CannotSort handling for books with missing or unmatched subjects.
- Added matching tests for category assignment, multi-match behavior, case-insensitive matching, whitespace normalization, and duplicate subject handling.
- Added existing Bragi output folder mode.
- Added category file detection for Bragi subject-list files.
- Added subject-list folder reading with trimming and deduplication.
- Added friendly validation errors for missing or empty Bragi output folders.
- Added warnings when some expected Bragi subject-list files are missing.
- Added tests for category detection and existing subject-list folder reading.
- Added reusable BragiCore subject extraction, categorization, and text export services.
- Added default Bragi category rules for Heimdall fresh subject-list generation.
- Added `BragiSubjectListGenerator` implementation for `IBragiSubjectListGenerator`.
- Added generation of category subject files, `NotCategorizedSubjects.txt`, and `RunSummary.txt`.
- Added tests for BragiCore extraction, categorization, export, generation, and UI dependency isolation.
- Added official FOLIO CSV reader using column names instead of numeric indexes.
- Added FOLIO CSV schema validation for required columns.
- Added conservative temporary summary extraction from `instances.notes`.
- Added optional internal debug field capture for selected FOLIO columns.
- Added tests for CSV loading, schema validation, summary extraction, and empty field handling.
- Added dependency injection, logging, and configuration loading.
- Added strongly typed Heimdall configuration models.
- Added initial domain models, value objects, result models, and application contracts.
- Added basic unit tests for domain model validation.
- Initialized project planning documentation.
- Documented confirmed MVP requirements.
- Documented planned clean architecture.
- Documented initial testing and release strategy.
- Added central user-friendly error model and message mapper.
- Added friendly CSV errors for missing columns, wrong file type, empty CSV, locked/unavailable files, and malformed records.
- Added friendly Bragi folder errors for missing folders, empty folders, and unreadable subject files.
- Added friendly export errors for missing/unwritable output folders and locked output files.
- Added export overwrite warnings when expected files already exist.
- Added UI-level exception mapping so users see recoverable messages instead of raw technical exceptions.
- Added technical exception logging from wizard pages and navigation.
- Added automated regression tests covering official CSV loading, required schema validation, Bragi fresh generation, existing subject-list folder mode, multi-category matching, preview removal, HTML rendering, CannotSort output, and RunSummary export.

### Changed

- Set Heimdall app version metadata to `1.0.0`.
- Set the release executable name to `Heimdall.exe`.
- Disabled trimming and ReadyToRun for conservative WinUI release publishing.
- Updated release documentation with finalized local publish, ZIP, and extraction-test commands.
- Updated README status from Phase 20 documentation polish to final `v1.0.0` release preparation.
- Documented that Heimdall skips empty selected categories instead of creating blank HTML email files.
- Clarified Phase 22 evidence handling and local log verification without committing sensitive local logs.
- Cleaned nullable warnings in CSV reading and Bragi category file detection.

### Verified

- Verified full fresh Bragi generation workflow from CSV selection through HTML export.
- Verified full existing Bragi folder workflow from CSV selection through HTML export.
- Verified generated HTML files, CannotSort output, RunSummary, and logs during manual end-to-end testing.
- Verified release screenshot set and documentation.
- Verified app remains recoverable after invalid input.
- Verified technical details are logged.
- Verified user-facing messages stay clear and staff-friendly.

### Known Limitations

- Direct email sending is not included.
- Outlook, SMTP, and Microsoft Graph integration are not included.
- ThirdIron integration is not included.
- Book cover images and API enrichment are not included.
- Editable email headers are not included.
- Legacy CSV schemas are not supported.
- Automatic scheduling is not included.
