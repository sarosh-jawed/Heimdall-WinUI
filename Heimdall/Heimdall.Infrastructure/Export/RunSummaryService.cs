using System.Text;
using Heimdall.Application.Contracts;
using Heimdall.Domain.Results;

namespace Heimdall.Infrastructure.Export;

public sealed class RunSummaryService : IRunSummaryService
{
    public string RenderText(RunSummary runSummary)
    {
        ArgumentNullException.ThrowIfNull(runSummary);

        var builder = new StringBuilder();

        builder.AppendLine("Heimdall Run Summary");
        builder.AppendLine($"Run Started At: {runSummary.RunStartedAt:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Run Ended At: {runSummary.RunEndedAt:yyyy-MM-dd HH:mm:ss}");

        if (runSummary.Duration is not null)
        {
            builder.AppendLine($"Duration: {runSummary.Duration}");
        }

        builder.AppendLine($"Source CSV Path: {runSummary.SourceCsvPath}");
        builder.AppendLine($"Subject List Mode: {runSummary.SubjectListMode}");
        builder.AppendLine($"Subject List Folder Path: {runSummary.SubjectListFolderPath}");
        builder.AppendLine($"Total Records Read: {runSummary.TotalRecordsRead}");
        builder.AppendLine($"Cannot Sort Count: {runSummary.CannotSortCount}");
        builder.AppendLine();

        builder.AppendLine("Selected Categories:");
        foreach (var category in runSummary.SelectedCategories)
        {
            builder.AppendLine($"- {category.Value}");
        }

        builder.AppendLine();
        builder.AppendLine("Matched Record Counts:");
        foreach (var item in runSummary.MatchedRecordCounts.OrderBy(item => item.Key.Value, StringComparer.OrdinalIgnoreCase))
        {
            builder.AppendLine($"- {item.Key.Value}: {item.Value}");
        }

        builder.AppendLine();
        builder.AppendLine("Generated Files:");
        foreach (var file in runSummary.GeneratedFiles)
        {
            builder.AppendLine($"- {file.Value}");
        }

        if (runSummary.Warnings.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Warnings:");
            foreach (var warning in runSummary.Warnings)
            {
                builder.AppendLine($"- {warning}");
            }
        }

        return builder.ToString();
    }
}
