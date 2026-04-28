# Heimdall WinUI Architecture

## Architectural Goal

Heimdall WinUI must be a professional, testable, maintainable desktop application.

The application should not be a direct copy of the legacy console utility. The legacy project is a behavior reference only.

## Planned Solution Structure

Heimdall-WinUI/
│
├── Heimdall/
│   ├── Heimdall.sln
│   ├── Heimdall.App.WinUI/
│   ├── Heimdall.Domain/
│   ├── Heimdall.Application/
│   ├── Heimdall.Infrastructure/
│   ├── Heimdall.BragiCore/
│   └── Heimdall.Tests/
│
├── docs/
│   ├── REQUIREMENTS.md
│   ├── ARCHITECTURE.md
│   ├── TEST-CASES.md
│   └── RELEASE.md
│
├── README.md
├── CHANGELOG.md
├── LICENSE.txt
├── .editorconfig
└── .gitignore