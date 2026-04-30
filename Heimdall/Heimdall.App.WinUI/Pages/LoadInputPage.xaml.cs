using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Heimdall.App.WinUI.Navigation;
using Heimdall.App.WinUI.Services;
using Heimdall.Application.Contracts;
using Heimdall.Application.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Heimdall.Application.Errors;
using Microsoft.Extensions.Logging;

namespace Heimdall.App.WinUI.Pages;

public sealed partial class LoadInputPage : Page, IWizardStepPage
{
    private readonly IFilePickerService _filePickerService;
    private readonly IWorkflowOrchestrator _workflowOrchestrator;
    private readonly WizardSessionStore _sessionStore;
    private readonly IUserMessageService _userMessageService;
    private readonly ILogger<LoadInputPage> _logger;

    public LoadInputPage()
    {
        InitializeComponent();

        _filePickerService = App.Services.GetRequiredService<IFilePickerService>();
        _workflowOrchestrator = App.Services.GetRequiredService<IWorkflowOrchestrator>();
        _sessionStore = App.Services.GetRequiredService<WizardSessionStore>();
        _userMessageService = App.Services.GetRequiredService<IUserMessageService>();
        _logger = App.Services.GetRequiredService<ILogger<LoadInputPage>>();

        Loaded += LoadInputPage_Loaded;
    }

    public async Task<WizardStepResult> OnNextAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_sessionStore.SourceCsvPath))
        {
            ShowError("CSV required", "Select the official FOLIO CSV file before continuing.");
            return WizardStepResult.Failure("CSV required", "Select the official FOLIO CSV file before continuing.");
        }

        if (string.IsNullOrWhiteSpace(_sessionStore.OutputFolderPath))
        {
            ShowError("Output folder required", "Select the output folder before continuing.");
            return WizardStepResult.Failure("Output folder required", "Select the output folder before continuing.");
        }

        if (!File.Exists(_sessionStore.SourceCsvPath))
        {
            ShowError("CSV file not found", "The selected CSV file no longer exists. Select it again.");
            return WizardStepResult.Failure("CSV file not found", "The selected CSV file no longer exists. Select it again.");
        }

        if (!Directory.Exists(_sessionStore.OutputFolderPath))
        {
            ShowError("Output folder not found", "The selected output folder no longer exists. Select it again.");
            return WizardStepResult.Failure("Output folder not found", "The selected output folder no longer exists. Select it again.");
        }

        try
        {
            string selectedCsvPath = _sessionStore.SourceCsvPath;
            string selectedOutputFolder = _sessionStore.OutputFolderPath;

            await _workflowOrchestrator.LoadCsvAsync(selectedCsvPath, cancellationToken);

            _sessionStore.SourceCsvPath = selectedCsvPath;
            _sessionStore.OutputFolderPath = selectedOutputFolder;

            ShowSuccess("CSV loaded", "The official FOLIO CSV was loaded successfully.");
            return WizardStepResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CSV load failed from Load Input page.");

            UserMessage message = _userMessageService.BuildMessage(
                ex,
                "CSV load failed",
                "Heimdall could not load the selected CSV. Check that it is the official FOLIO CSV and try again.");

            ShowError(message.Title, message.Message);
            return WizardStepResult.Failure(message.Title, message.Message);
        }
    }

    private void LoadInputPage_Loaded(object sender, RoutedEventArgs e)
    {
        RefreshDisplayedPaths();
    }

    private async void SelectCsvButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetPickerButtonsEnabled(false);

            string? selectedPath = await _filePickerService.PickCsvFileAsync();

            if (selectedPath is null)
            {
                ShowInfo("Selection canceled", "No CSV file was selected.");
                return;
            }

            if (!Path.GetExtension(selectedPath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ShowError("Invalid file type", "Please select an official FOLIO CSV file with the .csv extension.");
                return;
            }

            _sessionStore.SourceCsvPath = selectedPath;
            CsvPathTextBox.Text = selectedPath;

            ShowSuccess("CSV selected", "The official FOLIO CSV path has been saved for the workflow.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CSV picker failed.");

            UserMessage message = _userMessageService.BuildMessage(
                ex,
                "CSV selection failed",
                "Heimdall could not open the CSV picker. Try again or restart the app.");

            ShowError(message.Title, message.Message);
        }
        finally
        {
            SetPickerButtonsEnabled(true);
        }
    }

    private async void SelectOutputFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SetPickerButtonsEnabled(false);

            string? selectedPath = await _filePickerService.PickOutputFolderAsync();

            if (selectedPath is null)
            {
                ShowInfo("Selection canceled", "No output folder was selected.");
                return;
            }

            _sessionStore.OutputFolderPath = selectedPath;
            OutputFolderTextBox.Text = selectedPath;

            ShowSuccess("Output folder selected", "Generated files will be saved to the selected output folder.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Output folder picker failed.");

            UserMessage message = _userMessageService.BuildMessage(
                ex,
                "Output folder selection failed",
                "Heimdall could not open the output folder picker. Try again or restart the app.");

            ShowError(message.Title, message.Message);
        }
        finally
        {
            SetPickerButtonsEnabled(true);
        }
    }

    private void RefreshDisplayedPaths()
    {
        CsvPathTextBox.Text = _sessionStore.SourceCsvPath ?? string.Empty;
        OutputFolderTextBox.Text = _sessionStore.OutputFolderPath ?? string.Empty;
    }

    private void SetPickerButtonsEnabled(bool isEnabled)
    {
        SelectCsvButton.IsEnabled = isEnabled;
        SelectOutputFolderButton.IsEnabled = isEnabled;
    }

    private void ShowSuccess(string title, string message) => ShowStatus(InfoBarSeverity.Success, title, message);

    private void ShowInfo(string title, string message) => ShowStatus(InfoBarSeverity.Informational, title, message);

    private void ShowError(string title, string message) => ShowStatus(InfoBarSeverity.Error, title, message);

    private void ShowStatus(InfoBarSeverity severity, string title, string message)
    {
        PathStatusInfoBar.Severity = severity;
        PathStatusInfoBar.Title = title;
        PathStatusInfoBar.Message = message;
        PathStatusInfoBar.IsOpen = true;
    }
}
