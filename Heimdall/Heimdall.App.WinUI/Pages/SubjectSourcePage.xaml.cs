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

namespace Heimdall.App.WinUI.Pages;

public sealed partial class SubjectSourcePage : Page, IWizardStepPage
{
    private readonly IFilePickerService _filePickerService;
    private readonly IWorkflowOrchestrator _workflowOrchestrator;
    private readonly WizardSessionStore _sessionStore;

    public SubjectSourcePage()
    {
        InitializeComponent();

        _filePickerService = App.Services.GetRequiredService<IFilePickerService>();
        _workflowOrchestrator = App.Services.GetRequiredService<IWorkflowOrchestrator>();
        _sessionStore = App.Services.GetRequiredService<WizardSessionStore>();

        Loaded += SubjectSourcePage_Loaded;
    }

    public async Task<WizardStepResult> OnNextAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (ExistingFolderRadioButton.IsChecked == true)
            {
                return await LoadExistingFolderModeAsync(cancellationToken);
            }

            return await GenerateFreshModeAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            ShowError("Subject-list source failed", ex.Message);
            return WizardStepResult.Failure("Subject-list source failed", ex.Message);
        }
    }

    private async Task<WizardStepResult> GenerateFreshModeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_sessionStore.SourceCsvPath))
        {
            return WizardStepResult.Failure("CSV required", "Load the official FOLIO CSV before generating fresh Bragi subject lists.");
        }

        if (string.IsNullOrWhiteSpace(_sessionStore.OutputFolderPath))
        {
            return WizardStepResult.Failure("Output folder required", "Select an output folder before generating fresh Bragi subject lists.");
        }

        string workingSubjectFolder = Path.Combine(_sessionStore.OutputFolderPath, "BragiSubjectLists");

        await _workflowOrchestrator.GenerateFreshSubjectListsAsync(
            _sessionStore.SourceCsvPath,
            workingSubjectFolder,
            cancellationToken);

        ShowSuccess("Fresh subject lists generated", $"Bragi subject lists were generated in {workingSubjectFolder}.");
        return WizardStepResult.Success();
    }

    private async Task<WizardStepResult> LoadExistingFolderModeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_sessionStore.SubjectListFolderPath))
        {
            ShowError("Existing Bragi folder required", "Select an existing Bragi output folder before continuing.");
            return WizardStepResult.Failure("Existing Bragi folder required", "Select an existing Bragi output folder before continuing.");
        }

        if (!Directory.Exists(_sessionStore.SubjectListFolderPath))
        {
            ShowError("Existing Bragi folder not found", "The selected existing Bragi folder no longer exists. Select it again.");
            return WizardStepResult.Failure("Existing Bragi folder not found", "The selected existing Bragi folder no longer exists. Select it again.");
        }

        await _workflowOrchestrator.LoadExistingSubjectListsAsync(
            _sessionStore.SubjectListFolderPath,
            cancellationToken);

        ShowSuccess("Existing subject lists loaded", "Heimdall loaded the existing Bragi subject-list folder.");
        return WizardStepResult.Success();
    }

    private void SubjectSourcePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_sessionStore.SubjectListMode.Equals("ExistingFolder", StringComparison.OrdinalIgnoreCase))
        {
            ExistingFolderRadioButton.IsChecked = true;
        }
        else
        {
            GenerateFreshRadioButton.IsChecked = true;
            _sessionStore.SubjectListMode = "GenerateFresh";
        }

        ExistingBragiFolderTextBox.Text = _sessionStore.SubjectListFolderPath ?? string.Empty;
        UpdateExistingFolderControls();
    }

    private void SubjectSourceRadioButton_Checked(object sender, RoutedEventArgs e)
    {
        if (ExistingFolderRadioButton is null || GenerateFreshRadioButton is null)
        {
            return;
        }

        _sessionStore.SubjectListMode = ExistingFolderRadioButton.IsChecked == true
            ? "ExistingFolder"
            : "GenerateFresh";

        UpdateExistingFolderControls();
    }

    private async void SelectExistingBragiFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SelectExistingBragiFolderButton.IsEnabled = false;

            string? selectedPath = await _filePickerService.PickExistingBragiFolderAsync();

            if (selectedPath is null)
            {
                ShowInfo("Selection canceled", "No existing Bragi output folder was selected.");
                return;
            }

            _sessionStore.SubjectListMode = "ExistingFolder";
            _sessionStore.SubjectListFolderPath = selectedPath;

            ExistingFolderRadioButton.IsChecked = true;
            ExistingBragiFolderTextBox.Text = selectedPath;

            ShowSuccess("Existing Bragi folder selected", "Heimdall will read subject-list files from the selected folder.");
        }
        catch (Exception ex)
        {
            ShowError("Existing Bragi folder selection failed", ex.Message);
        }
        finally
        {
            UpdateExistingFolderControls();
        }
    }

    private void UpdateExistingFolderControls()
    {
        bool useExistingFolder = ExistingFolderRadioButton.IsChecked == true;

        ExistingBragiFolderTextBox.IsEnabled = useExistingFolder;
        SelectExistingBragiFolderButton.IsEnabled = useExistingFolder;
    }

    private void ShowSuccess(string title, string message) => ShowStatus(InfoBarSeverity.Success, title, message);

    private void ShowInfo(string title, string message) => ShowStatus(InfoBarSeverity.Informational, title, message);

    private void ShowError(string title, string message) => ShowStatus(InfoBarSeverity.Error, title, message);

    private void ShowStatus(InfoBarSeverity severity, string title, string message)
    {
        SubjectSourceStatusInfoBar.Severity = severity;
        SubjectSourceStatusInfoBar.Title = title;
        SubjectSourceStatusInfoBar.Message = message;
        SubjectSourceStatusInfoBar.IsOpen = true;
    }
}
