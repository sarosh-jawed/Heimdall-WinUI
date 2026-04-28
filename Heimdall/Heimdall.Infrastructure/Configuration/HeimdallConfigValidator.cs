using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;

namespace Heimdall.Infrastructure.Configuration;

public sealed class HeimdallConfigValidator : IHeimdallConfigValidator
{
    public void ValidateAndThrow(HeimdallConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var errors = new List<string>();

        RequireValue(config.CsvColumns.TitleColumnName, "CSV title column name", errors);
        RequireValue(config.CsvColumns.AuthorColumnName, "CSV author column name", errors);
        RequireValue(config.CsvColumns.SummaryColumnName, "CSV summary column name", errors);
        RequireValue(config.CsvColumns.SubjectColumnName, "CSV subject column name", errors);
        RequireValue(config.CsvColumns.RecordIdColumnName, "CSV record ID column name", errors);

        if (!config.SubjectLists.AllowGenerateFreshLists && !config.SubjectLists.AllowExistingSubjectListFolder)
        {
            errors.Add("At least one subject-list source mode must be enabled.");
        }

        if (config.SubjectLists.RequiredCategoryFiles.Count == 0)
        {
            errors.Add("At least one required category subject file must be configured.");
        }

        RequireValue(config.Output.DateFormat, "output date format", errors);
        RequireValue(config.Output.CategoryFileNameTemplate, "category file name template", errors);
        RequireValue(config.Output.CannotSortFileNameTemplate, "CannotSort file name template", errors);
        RequireValue(config.Output.RunSummaryFileNameTemplate, "run summary file name template", errors);

        RequireValue(config.Logging.LogFolderName, "log folder name", errors);
        RequireValue(config.Logging.LogFileNameTemplate, "log file name template", errors);

        if (config.Logging.RetainedFileCountLimit <= 0)
        {
            errors.Add("Retained log file count must be greater than zero.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException(
                "Heimdall configuration is invalid: " + string.Join(" ", errors));
        }
    }

    private static void RequireValue(string? value, string label, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add($"{label} is required.");
        }
    }
}
