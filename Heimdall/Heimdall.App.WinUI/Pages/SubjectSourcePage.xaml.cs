using System;
using Heimdall.App.WinUI.Services;
using Heimdall.Application.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Heimdall.App.WinUI.Pages;

public sealed partial class SubjectSourcePage : Page
{
    private readonly IFilePickerService _filePickerService;
    private readonly WizardSessionStore _sessionStore;

    public SubjectSourcePage()
    {
        InitializeComponent();

        _filePickerService = App.Services.GetRequiredService<IFilePickerService>();
        _sessionStore = App.Services.GetRequiredService<WizardSessionStore>();

        Loaded += SubjectSourcePage_Loaded;
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

    private void ShowSuccess(string title, string message)
    {
        ShowStatus(InfoBarSeverity.Success, title, message);
    }

    private void ShowInfo(string title, string message)
    {
        ShowStatus(InfoBarSeverity.Informational, title, message);
    }

    private void ShowError(string title, string message)
    {
        ShowStatus(InfoBarSeverity.Error, title, message);
    }

    private void ShowStatus(InfoBarSeverity severity, string title, string message)
    {
        SubjectSourceStatusInfoBar.Severity = severity;
        SubjectSourceStatusInfoBar.Title = title;
        SubjectSourceStatusInfoBar.Message = message;
        SubjectSourceStatusInfoBar.IsOpen = true;
    }
}
