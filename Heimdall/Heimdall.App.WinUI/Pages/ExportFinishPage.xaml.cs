using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Heimdall.Application.Configuration;
using Heimdall.Application.Contracts;
using Heimdall.Application.Workflow;
using Heimdall.Domain.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Heimdall.App.WinUI.Pages;

public sealed partial class ExportFinishPage : Page
{
    private readonly IWorkflowOrchestrator _workflowOrchestrator;
    private readonly WizardSessionStore _sessionStore;
    private readonly HeimdallConfig _config;

    private HtmlExportResult? _lastExportResult;

    public ExportFinishPage()
    {
        InitializeComponent();

        _workflowOrchestrator = App.Services.GetRequiredService<IWorkflowOrchestrator>();
        _sessionStore = App.Services.GetRequiredService<WizardSessionStore>();
        _config = App.Services.GetRequiredService<HeimdallConfig>();

        Loaded += ExportFinishPage_Loaded;
    }

    private void ExportFinishPage_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshPageState();
    }

    private async void ExportFilesButton_Click(object sender, RoutedEventArgs e)
    {
        if (_sessionStore.PreviewResult is null)
        {
            ShowError("Preview required", "Build the book preview before exporting final HTML files.");
            return;
        }

        if (string.IsNullOrWhiteSpace(_sessionStore.OutputFolderPath))
        {
            ShowError("Output folder required", "Go back to Load Input and select an output folder.");
            return;
        }

        if (!Directory.Exists(_sessionStore.OutputFolderPath))
        {
            ShowError("Output folder not found", "The selected output folder no longer exists. Select it again.");
            return;
        }

        try
        {
            SetBusy(true);

            HtmlExportResult result = await _workflowOrchestrator.ExportAsync(
                _sessionStore.OutputFolderPath,
                CancellationToken.None);

            _lastExportResult = result;

            RenderFinishState(result);

            ShowSuccess(
                "Export completed successfully",
                $"{result.GeneratedFiles.Count} file(s) were written directly into the selected output folder.");
        }
        catch (Exception ex)
        {
            ShowError("Export failed", ex.Message);
        }
        finally
        {
            SetBusy(false);
            RefreshOpenButtons();
        }
    }

    private void OpenOutputFolderButton_Click(object sender, RoutedEventArgs e)
    {
        string? outputFolder = _lastExportResult?.OutputFolder ?? _sessionStore.OutputFolderPath;
        OpenFolder(outputFolder, "Output folder unavailable", "The output folder could not be opened because it does not exist.");
    }

    private void OpenLogsButton_Click(object sender, RoutedEventArgs e)
    {
        string logFolder = ResolveLogFolder();
        Directory.CreateDirectory(logFolder);

        OpenFolder(logFolder, "Logs unavailable", "The log folder could not be opened.");
    }

    private void RefreshPageState()
    {
        OutputFolderTextBlock.Text = string.IsNullOrWhiteSpace(_sessionStore.OutputFolderPath)
            ? "No output folder selected yet."
            : $"Output folder: {_sessionStore.OutputFolderPath}";

        ExportFilesButton.IsEnabled =
            _sessionStore.PreviewResult is not null &&
            !string.IsNullOrWhiteSpace(_sessionStore.OutputFolderPath) &&
            Directory.Exists(_sessionStore.OutputFolderPath);

        if (_sessionStore.PreviewResult is null)
        {
            CompletionStatusTextBlock.Text = "Export has not been run yet. Build a preview first, then return here to export.";
            GeneratedFilesSummaryTextBlock.Text = "No preview is available yet. Go back to Select Categories and build the preview before exporting.";
        }
        else
        {
            CompletionStatusTextBlock.Text = "Ready to export. Click Export Files to generate the final HTML files and RunSummary.";
        }

        RefreshOpenButtons();
    }

    private void RenderFinishState(HtmlExportResult result)
    {
        CompletionStatusTextBlock.Text = "Export completed successfully.";
        OutputFolderTextBlock.Text = $"Output folder: {result.OutputFolder}";

        RenderRunSummary(result.RunSummary);
        RenderGeneratedFiles(result);
    }

    private void RenderRunSummary(RunSummary runSummary)
    {
        RunSummaryDetailsTextBlock.Text =
            $"Source CSV: {ValueOrNotProvided(runSummary.SourceCsvPath)}{Environment.NewLine}" +
            $"Subject-list mode: {ValueOrNotProvided(runSummary.SubjectListMode)}{Environment.NewLine}" +
            $"Subject-list folder: {ValueOrNotProvided(runSummary.SubjectListFolderPath)}{Environment.NewLine}" +
            $"Total records read: {runSummary.TotalRecordsRead}{Environment.NewLine}" +
            $"Run started: {runSummary.RunStartedAt:yyyy-MM-dd HH:mm:ss}{Environment.NewLine}" +
            $"Run ended: {runSummary.RunEndedAt:yyyy-MM-dd HH:mm:ss}";

        CannotSortCountTextBlock.Text = $"CannotSort count: {runSummary.CannotSortCount}";

        CategoryCountsListView.Items.Clear();

        foreach (var item in runSummary.MatchedRecordCounts.OrderBy(item => item.Key.Value, StringComparer.OrdinalIgnoreCase))
        {
            int removedCount = runSummary.RemovedRecordCounts.TryGetValue(item.Key, out int count)
                ? count
                : 0;

            CategoryCountsListView.Items.Add(
                $"{ToDisplayName(item.Key.Value)} — {item.Value} matched/active, {removedCount} removed");
        }

        CategoryCountsSummaryTextBlock.Text =
            $"{runSummary.MatchedRecordCounts.Count} selected categor{(runSummary.MatchedRecordCounts.Count == 1 ? "y" : "ies")} summarized.";
    }

    private void RenderGeneratedFiles(HtmlExportResult result)
    {
        GeneratedFilesListView.Items.Clear();

        foreach (string fileName in result.GeneratedFiles
                     .Select(file => file.Value)
                     .OrderBy(fileName => fileName, StringComparer.OrdinalIgnoreCase))
        {
            GeneratedFilesListView.Items.Add(fileName);
        }

        GeneratedFilesSummaryTextBlock.Text =
            $"{result.GeneratedFiles.Count} file(s) generated in {result.OutputFolder}";
    }

    private void SetBusy(bool isBusy)
    {
        ExportProgressRing.IsActive = isBusy;
        ExportProgressRing.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;

        ExportFilesButton.IsEnabled = !isBusy &&
            _sessionStore.PreviewResult is not null &&
            !string.IsNullOrWhiteSpace(_sessionStore.OutputFolderPath) &&
            Directory.Exists(_sessionStore.OutputFolderPath);

        OpenOutputFolderButton.IsEnabled = !isBusy &&
            !string.IsNullOrWhiteSpace(_sessionStore.OutputFolderPath) &&
            Directory.Exists(_sessionStore.OutputFolderPath);

        OpenLogsButton.IsEnabled = !isBusy;
    }

    private void RefreshOpenButtons()
    {
        string? outputFolder = _lastExportResult?.OutputFolder ?? _sessionStore.OutputFolderPath;

        OpenOutputFolderButton.IsEnabled =
            !string.IsNullOrWhiteSpace(outputFolder) &&
            Directory.Exists(outputFolder);

        OpenLogsButton.IsEnabled = true;
    }

    private void OpenFolder(string? folderPath, string title, string message)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            ShowError(title, message);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowError(title, ex.Message);
        }
    }

    private string ResolveLogFolder()
    {
        string rootFolder = _config.Logging.UseDocumentsFolder
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        return Path.Combine(rootFolder, _config.Logging.LogFolderName);
    }

    private static string ValueOrNotProvided(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "Not provided"
            : value;
    }

    private static string ToDisplayName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Category";
        }

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
    }

    private void ShowSuccess(string title, string message)
    {
        ShowStatus(InfoBarSeverity.Success, title, message);
    }

    private void ShowError(string title, string message)
    {
        ShowStatus(InfoBarSeverity.Error, title, message);
    }

    private void ShowStatus(InfoBarSeverity severity, string title, string message)
    {
        ExportStatusInfoBar.Severity = severity;
        ExportStatusInfoBar.Title = title;
        ExportStatusInfoBar.Message = message;
        ExportStatusInfoBar.IsOpen = true;
    }
}
