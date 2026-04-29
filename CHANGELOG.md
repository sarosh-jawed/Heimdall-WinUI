# Changelog

All notable changes to Heimdall WinUI will be documented in this file.

## [Unreleased]

### Added

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







