using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Heimdall.App.WinUI.Navigation;
using Heimdall.App.WinUI.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Heimdall.Application.Contracts;
using Heimdall.Application.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Heimdall.App.WinUI;

/// <summary>
/// Main desktop shell for Heimdall's step-by-step wizard.
/// Business workflow logic stays in page/view workflow services; this class owns navigation and shell state.
/// </summary>
[SupportedOSPlatform("windows10.0.17763.0")]
public sealed partial class MainWindow : Window
{
    private readonly IReadOnlyList<WizardStepDefinition> _steps =
        new[]
        {
            new WizardStepDefinition(
                "Start",
                "Review what Heimdall will do before loading any files.",
                typeof(StartPage)),

            new WizardStepDefinition(
                "Load Input",
                "Select the official FOLIO CSV file and the output folder.",
                typeof(LoadInputPage)),

            new WizardStepDefinition(
                "Subject Source",
                "Choose whether to generate fresh Bragi subject lists or use an existing Bragi output folder.",
                typeof(SubjectSourcePage)),

            new WizardStepDefinition(
                "Select Categories",
                "Choose the broad subject categories that should receive generated email files.",
                typeof(CategorySelectionPage)),

            new WizardStepDefinition(
                "Preview Books",
                "Review matched books by category and remove books from specific category previews if needed.",
                typeof(PreviewBooksPage)),

            new WizardStepDefinition(
                "Export & Finish",
                "Generate HTML files, CannotSort output, run summary, and logs.",
                typeof(ExportFinishPage))
        };

    private int _currentStepIndex;
    private readonly IUserMessageService _userMessageService;
    private readonly ILogger<MainWindow> _logger;
    public static IntPtr WindowHandle { get; private set; }

    public MainWindow()
    {
        InitializeComponent();

        WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
        _userMessageService = App.Services.GetRequiredService<IUserMessageService>();
        _logger = App.Services.GetRequiredService<ILogger<MainWindow>>();

        StepsListView.Items.Clear();

        for (int index = 0; index < _steps.Count; index++)
        {
            StepsListView.Items.Add($"{index + 1}. {_steps[index].Title}");
        }

        NavigateToStep(0);
    }

    private async void BackButton_Click(object _, RoutedEventArgs _1)
    {
        if (_currentStepIndex <= 0)
        {
            return;
        }

        await NavigateToStepAsync(_currentStepIndex - 1, validateCurrentStep: false);
    }

    private async void NextButton_Click(object _, RoutedEventArgs _1)
    {
        if (_currentStepIndex >= _steps.Count - 1)
        {
            return;
        }

        await NavigateToStepAsync(_currentStepIndex + 1, validateCurrentStep: true);
    }

    private async Task NavigateToStepAsync(int targetStepIndex, bool validateCurrentStep)
    {
        try
        {
            SetBusy(true, "Working...");

            if (validateCurrentStep && ContentFrame.Content is IWizardStepPage wizardStepPage)
            {
                WizardStepResult result = await wizardStepPage.OnNextAsync(CancellationToken.None);

                if (!result.CanContinue)
                {
                    ShowError(
                        result.ErrorTitle ?? "Cannot continue",
                        result.ErrorMessage ?? "Please resolve the current step before continuing.");

                    return;
                }
            }

            NavigateToStep(targetStepIndex);
            HideStatusMessage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wizard navigation failed. TargetStepIndex={TargetStepIndex}", targetStepIndex);

            UserMessage message = _userMessageService.BuildMessage(
                ex,
                "Navigation failed",
                "Heimdall could not continue to the next step. Resolve the current step and try again.");

            ShowError(message.Title, message.Message);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void NavigateToStep(int targetStepIndex)
    {
        if (targetStepIndex < 0 || targetStepIndex >= _steps.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetStepIndex),
                "The requested wizard step does not exist.");
        }

        _currentStepIndex = targetStepIndex;

        WizardStepDefinition step = _steps[_currentStepIndex];

        ContentFrame.Navigate(step.PageType);

        StepTitleTextBlock.Text = step.Title;
        StepDescriptionTextBlock.Text = step.Description;
        ProgressTextBlock.Text = $"Step {_currentStepIndex + 1} of {_steps.Count}";
        WizardProgressBar.Value = _currentStepIndex + 1;
        StepsListView.SelectedIndex = _currentStepIndex;

        UpdateNavigationButtons();
    }

    private void SetBusy(bool isBusy, string? message = null)
    {
        BusyProgressRing.IsActive = isBusy;
        BusyProgressRing.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;

        BusyTextBlock.Text = isBusy ? message ?? "Working..." : string.Empty;
        BusyTextBlock.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;

        BackButton.IsEnabled = !isBusy && _currentStepIndex > 0;
        NextButton.IsEnabled = !isBusy && _currentStepIndex < _steps.Count - 1;
    }

    private void UpdateNavigationButtons()
    {
        BackButton.IsEnabled = _currentStepIndex > 0;
        NextButton.IsEnabled = _currentStepIndex < _steps.Count - 1;
        NextButton.Content = _currentStepIndex == _steps.Count - 2
            ? "Continue to Finish"
            : "Next";
    }

    private void ShowError(string title, string message)
    {
        StatusInfoBar.Severity = InfoBarSeverity.Error;
        StatusInfoBar.Title = title;
        StatusInfoBar.Message = message;
        StatusInfoBar.IsOpen = true;
    }

    private void HideStatusMessage()
    {
        StatusInfoBar.IsOpen = false;
    }

    private sealed record WizardStepDefinition(
        string Title,
        string Description,
        Type PageType);
}
