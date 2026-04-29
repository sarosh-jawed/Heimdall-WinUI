using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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

    private HtmlExportResult? _lastExportResult;

    public ExportFinishPage()
    {
        InitializeComponent();

        _workflowOrchestrator = App.Services.GetRequiredService<IWorkflowOrchestrator>();
        _sessionStore = App.Services.GetRequiredService<WizardSessionStore>();

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

            RenderGeneratedFiles(result);

            ShowSuccess(
                "Export complete",
                $"{result.GeneratedFiles.Count} file(s) were written directly into the selected output folder.");
        }
        catch (Exception ex)
        {
            ShowError("Export failed", ex.Message);
        }
        finally
        {
            SetBusy(false);
            RefreshOpenOutputFolderButton();
        }
    }

    private void OpenOutputFolderButton_Click(object sender, RoutedEventArgs e)
    {
        string? outputFolder = _lastExportResult?.OutputFolder ?? _sessionStore.OutputFolderPath;

        if (string.IsNullOrWhiteSpace(outputFolder) || !Directory.Exists(outputFolder))
        {
            ShowError("Output folder unavailable", "The output folder could not be opened because it does not exist.");
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = outputFolder,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowError("Could not open folder", ex.Message);
        }
    }

    private void RefreshPageState()
    {
        OutputFolderTextBlock.Text = string.IsNullOrWhiteSpace(_sessionStore.OutputFolderPath)
            ? "No output folder selected yet."
            : _sessionStore.OutputFolderPath;

        ExportFilesButton.IsEnabled =
            _sessionStore.PreviewResult is not null &&
            !string.IsNullOrWhiteSpace(_sessionStore.OutputFolderPath) &&
            Directory.Exists(_sessionStore.OutputFolderPath);

        RefreshOpenOutputFolderButton();

        if (_sessionStore.PreviewResult is null)
        {
            GeneratedFilesSummaryTextBlock.Text = "No preview is available yet. Go back to Select Categories and build the preview before exporting.";
        }
    }

    private void RefreshOpenOutputFolderButton()
    {
        string? outputFolder = _lastExportResult?.OutputFolder ?? _sessionStore.OutputFolderPath;

        OpenOutputFolderButton.IsEnabled =
            !string.IsNullOrWhiteSpace(outputFolder) &&
            Directory.Exists(outputFolder);
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
