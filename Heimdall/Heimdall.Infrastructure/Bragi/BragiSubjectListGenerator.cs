using Heimdall.Application.Contracts;
using Heimdall.BragiCore.Categorization;
using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Export;
using Heimdall.BragiCore.Extraction;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;

namespace Heimdall.Infrastructure.Bragi;

public sealed class BragiSubjectListGenerator : IBragiSubjectListGenerator
{
    private readonly ICsvBookRecordReader _csvBookRecordReader;
    private readonly BragiCoreOptions _options;
    private readonly SubjectExtractionService _subjectExtractionService;
    private readonly CategorizationService _categorizationService;
    private readonly TextExportService _textExportService;

    public BragiSubjectListGenerator(
        ICsvBookRecordReader csvBookRecordReader,
        BragiCoreOptions options,
        SubjectExtractionService subjectExtractionService,
        CategorizationService categorizationService,
        TextExportService textExportService)
    {
        _csvBookRecordReader = csvBookRecordReader;
        _options = options;
        _subjectExtractionService = subjectExtractionService;
        _categorizationService = categorizationService;
        _textExportService = textExportService;
    }

    public async Task<BragiGenerationResult> GenerateAsync(
        string sourceCsvPath,
        string outputFolder,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCsvPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFolder);

        var csvLoadResult = await _csvBookRecordReader.ReadAsync(sourceCsvPath, cancellationToken);

        var extractionResult = _subjectExtractionService.ExtractFromBookRecords(
            csvLoadResult.Books,
            _options.BehaviorOptions);

        var categorizationResult = _categorizationService.Categorize(
            extractionResult,
            _options.CategoryRules,
            _options.BehaviorOptions);

        var exportResult = await _textExportService.ExportAsync(
            outputFolder,
            _options.CategoryRules,
            categorizationResult,
            _options,
            cancellationToken);

        var categorySubjectLists = BuildCategorySubjectLists(exportResult);

        var warnings = csvLoadResult.Warnings
            .Concat(extractionResult.Warnings)
            .ToArray();

        return new BragiGenerationResult(
            Success: true,
            OutputFolder: outputFolder,
            SubjectListLoadResult: new SubjectListLoadResult(categorySubjectLists, warnings),
            Warnings: warnings);
    }

    private IReadOnlyList<CategorySubjectList> BuildCategorySubjectLists(BragiTextExportResult exportResult)
    {
        return _options.CategoryRules
            .Where(rule => rule.Enabled)
            .OrderBy(rule => rule.SortOrder)
            .Select(rule =>
            {
                var subjects = exportResult.CategorySubjects.TryGetValue(rule.Key, out var categorySubjects)
                    ? categorySubjects.Select(subject => new SubjectHeading(subject)).ToArray()
                    : Array.Empty<SubjectHeading>();

                return new CategorySubjectList(
                    new CategoryDefinition(rule.DisplayName, rule.OutputFileName),
                    subjects);
            })
            .ToArray();
    }
}
