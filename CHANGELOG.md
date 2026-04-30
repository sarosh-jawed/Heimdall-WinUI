# Changelog

All notable changes to Heimdall WinUI will be documented in this file.

## [Unreleased]

### Added

- Expanded the Export & Finish page into a completion summary.
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
- Prepared the UI structure for future file picker, category loading, preview, and export integration.

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
- Cleaned nullable warnings in CSV reading and Bragi category file detection.

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

- Central user-friendly error model and message mapper.
- Friendly CSV errors for missing columns, wrong file type, empty CSV, locked/unavailable files, and malformed records.
- Friendly Bragi folder errors for missing folders, empty folders, and unreadable subject files.
- Friendly export errors for missing/unwritable output folders and locked output files.
- Export overwrite warnings when expected files already exist.
- UI-level exception mapping so users see recoverable messages instead of raw technical exceptions.
- Technical exception logging from wizard pages and navigation.

### Verified
- App remains recoverable after invalid input.
- Technical details are logged.
- User-facing messages stay clear and staff-friendly.











