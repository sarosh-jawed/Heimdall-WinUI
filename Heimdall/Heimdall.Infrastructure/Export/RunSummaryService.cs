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
        builder.AppendLine("====================");
        builder.AppendLine();

        builder.AppendLine("Run Timing:");
        builder.AppendLine($"- Run Started At: {runSummary.RunStartedAt:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"- Run Ended At: {runSummary.RunEndedAt:yyyy-MM-dd HH:mm:ss}");

        if (runSummary.Duration is not null)
        {
            builder.AppendLine($"- Duration: {runSummary.Duration}");
        }

        builder.AppendLine();

        builder.AppendLine("Input:");
        builder.AppendLine($"- Source CSV Path: {ValueOrNotProvided(runSummary.SourceCsvPath)}");
        builder.AppendLine($"- Subject List Mode: {ValueOrNotProvided(runSummary.SubjectListMode)}");
        builder.AppendLine($"- Subject List Folder Path: {ValueOrNotProvided(runSummary.SubjectListFolderPath)}");
        builder.AppendLine();

        builder.AppendLine("Record Counts:");
        builder.AppendLine($"- Total Records Read: {runSummary.TotalRecordsRead}");
        builder.AppendLine($"- Cannot Sort Count: {runSummary.CannotSortCount}");
        builder.AppendLine();

        builder.AppendLine("Selected Categories:");
        if (runSummary.SelectedCategories.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var category in runSummary.SelectedCategories)
            {
                builder.AppendLine($"- {category.Value}");
            }
        }

        builder.AppendLine();

        builder.AppendLine("Matched Records by Category:");
        if (runSummary.MatchedRecordCounts.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var item in runSummary.MatchedRecordCounts.OrderBy(item => item.Key.Value, StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine($"- {item.Key.Value}: {item.Value}");
            }
        }

        builder.AppendLine();

        builder.AppendLine("Removed Records by Category:");
        if (runSummary.RemovedRecordCounts.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var item in runSummary.RemovedRecordCounts.OrderBy(item => item.Key.Value, StringComparer.OrdinalIgnoreCase))
            {
                builder.AppendLine($"- {item.Key.Value}: {item.Value}");
            }
        }

        builder.AppendLine();

        builder.AppendLine("Generated Files:");
        if (runSummary.GeneratedFiles.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var file in runSummary.GeneratedFiles)
            {
                builder.AppendLine($"- {file.Value}");
            }
        }

        builder.AppendLine();

        builder.AppendLine("Warnings:");
        if (runSummary.Warnings.Count == 0)
        {
            builder.AppendLine("- None.");
        }
        else
        {
            foreach (var warning in runSummary.Warnings)
            {
                builder.AppendLine($"- {warning}");
            }
        }

        return builder.ToString();
    }

    private static string ValueOrNotProvided(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Not provided"
            : value;
    }
}
