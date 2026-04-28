using Heimdall.BragiCore.Categorization;
using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Extraction;

namespace Heimdall.Tests.Bragi;

public sealed class BragiCoreCategorizationTests
{
    [Fact]
    public void Categorize_AllowsMultiMatch_WhenConfigured()
    {
        var extractionResult = new SubjectExtractionResult(
            new[]
            {
                new ExtractedSubject("History of education", "history of education", "Book", "id-1", 2)
            },
            TotalRecordsRead: 1,
            BlankOrIgnoredCount: 0,
            DuplicateCount: 0,
            Warnings: Array.Empty<string>());

        var service = new CategorizationService();

        var result = service.Categorize(
            extractionResult,
            DefaultCategoryRules.Create(),
            new BehaviorOptions { AllowMultiMatch = true });

        Assert.Contains(result.Assignments, assignment => assignment.CategoryRule.Key == "history");
        Assert.Contains(result.Assignments, assignment => assignment.CategoryRule.Key == "education");
    }
}
