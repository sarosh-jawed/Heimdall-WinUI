# Changelog

All notable changes to Heimdall WinUI will be documented in this file.

## [Unreleased]

### Added

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



