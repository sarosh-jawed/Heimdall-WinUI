using System;
using System.IO;
using Heimdall.App.WinUI.Services;
using Heimdall.Application.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Heimdall.App.WinUI.Pages;

public sealed partial class LoadInputPage : Page
{
    private readonly IFilePickerService _filePickerService;
    private readonly WizardSessionStore _sessionStore;

    public LoadInputPage()
    {
        InitializeComponent();

        _filePickerService = App.Services.GetRequiredService<IFilePickerService>();
        _sessionStore = App.Services.GetRequiredService<WizardSessionStore>();

        Loaded += LoadInputPage_Loaded;
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
            ShowError("CSV selection failed", ex.Message);
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
            ShowError("Output folder selection failed", ex.Message);
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
        PathStatusInfoBar.Severity = severity;
        PathStatusInfoBar.Title = title;
        PathStatusInfoBar.Message = message;
        PathStatusInfoBar.IsOpen = true;
    }
}
