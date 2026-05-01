using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Heimdall.App.WinUI.Navigation;
using Heimdall.App.WinUI.Pages;
using Heimdall.Application.Contracts;
using Heimdall.Application.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace Heimdall.App.WinUI;

/// <summary>
/// Main desktop shell for Heimdall's guided wizard.
/// This class owns window polish, sidebar navigation, and shell state.
/// Business workflow logic stays in the application and infrastructure layers.
/// </summary>
[SupportedOSPlatform("windows10.0.17763.0")]
public sealed partial class MainWindow : Window
{
    private const int InitialWindowWidth = 1240;
    private const int InitialWindowHeight = 820;

    private readonly IReadOnlyList<WizardStepDefinition> _steps =
        new[]
        {
            new WizardStepDefinition(
                "Start",
                "Overview",
                "Review what Heimdall will create before loading any files.",
                "Start workflow",
                typeof(StartPage)),

            new WizardStepDefinition(
                "Select FOLIO CSV",
                "Input files",
                "Select the official FOLIO CSV file and choose where generated files should be saved.",
                "Continue",
                typeof(LoadInputPage)),

            new WizardStepDefinition(
                "Choose subject-list source",
                "Bragi lists",
                "Generate fresh Bragi subject lists or use an existing Bragi output folder.",
                "Prepare subject lists",
                typeof(SubjectSourcePage)),

            new WizardStepDefinition(
                "Select email categories",
                "Categories",
                "Choose the broad subject categories that should receive generated HTML email files.",
                "Preview matched books",
                typeof(CategorySelectionPage)),

            new WizardStepDefinition(
                "Preview matched books",
                "Review",
                "Review matched books by category and remove books from specific category previews if needed.",
                "Continue to finish",
                typeof(PreviewBooksPage)),

            new WizardStepDefinition(
                "Generate HTML emails",
                "Export",
                "Generate category HTML files, CannotSort output, run summary, and logs.",
                "Done",
                typeof(ExportFinishPage))
        };

    private readonly IUserMessageService _userMessageService;
    private readonly ILogger<MainWindow> _logger;

    private AppWindow? _appWindow;
    private int _currentStepIndex;
    private int _furthestUnlockedStepIndex;
    private bool _isUpdatingStepSelection;

    public static IntPtr WindowHandle { get; private set; }

    public MainWindow()
    {
        InitializeComponent();

        _userMessageService = App.Services.GetRequiredService<IUserMessageService>();
        _logger = App.Services.GetRequiredService<ILogger<MainWindow>>();

        ConfigureWindow();
        BuildStepRail();
        NavigateToStep(0);
    }

    private void ConfigureWindow()
    {
        WindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);

        var windowId = Win32Interop.GetWindowIdFromWindow(WindowHandle);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        _appWindow.Title = "Heimdall";
        _appWindow.Resize(new SizeInt32(InitialWindowWidth, InitialWindowHeight));

        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Heimdall.ico");

        if (File.Exists(iconPath))
        {
            _appWindow.SetIcon(iconPath);
        }
        else
        {
            _logger.LogWarning("Heimdall app icon was not found. IconPath={IconPath}", iconPath);
        }
    }

    private void BuildStepRail()
    {
        StepsListView.Items.Clear();

        for (int index = 0; index < _steps.Count; index++)
        {
            var step = _steps[index];

            var item = new ListViewItem
            {
                Tag = index,
                Padding = new Thickness(12, 10, 12, 10),
                IsEnabled = index == 0,
                Content = BuildStepRailContent(index, step)
            };

            ToolTipService.SetToolTip(item, step.Description);
            StepsListView.Items.Add(item);
        }

        UpdateStepRailItems();
    }

    private StackPanel BuildStepRailContent(int index, WizardStepDefinition step)
    {
        Brush titleBrush = (Brush)Microsoft.UI.Xaml.Application.Current.Resources["HeimdallSidebarTextBrush"];
        Brush subtitleBrush = (Brush)Microsoft.UI.Xaml.Application.Current.Resources["HeimdallSidebarMutedTextBrush"];

        return new StackPanel
        {
            Spacing = 2,
            Padding = new Thickness(8, 6, 8, 6),
            Children =
        {
            new TextBlock
            {
                Text = $"{index + 1}. {step.Title}",
                FontWeight = FontWeights.SemiBold,
                Foreground = titleBrush,
                TextWrapping = TextWrapping.Wrap
            },
            new TextBlock
            {
                Text = step.ShortLabel,
                FontSize = 12,
                Foreground = subtitleBrush,
                TextWrapping = TextWrapping.WrapWholeWords
            }
        }
        };
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

    private async void StepsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingStepSelection)
        {
            return;
        }

        if (StepsListView.SelectedItem is not ListViewItem selectedItem ||
            selectedItem.Tag is not int targetStepIndex)
        {
            return;
        }

        if (targetStepIndex == _currentStepIndex)
        {
            return;
        }

        if (targetStepIndex > _furthestUnlockedStepIndex)
        {
            ShowInfo(
                "Step not available yet",
                "Complete the current step with the main action button before jumping ahead.");

            SelectCurrentStepInRail();
            return;
        }

        await NavigateToStepAsync(targetStepIndex, validateCurrentStep: false);
    }

    private async Task NavigateToStepAsync(int targetStepIndex, bool validateCurrentStep)
    {
        try
        {
            SetBusy(true, GetBusyMessage(targetStepIndex));

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

            if (targetStepIndex > _furthestUnlockedStepIndex)
            {
                _furthestUnlockedStepIndex = targetStepIndex;
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
                "Heimdall could not continue. Resolve the current step and try again.");

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

        UpdateStepRailItems();
        SelectCurrentStepInRail();
        UpdateNavigationButtons();
    }

    private void UpdateStepRailItems()
    {
        for (int index = 0; index < StepsListView.Items.Count; index++)
        {
            if (StepsListView.Items[index] is not ListViewItem item)
            {
                continue;
            }

            bool isUnlocked = index <= _furthestUnlockedStepIndex;

            // Keep future steps readable, but visually mark them as locked.
            // Navigation is blocked in StepsListView_SelectionChanged until the step is unlocked.
            item.IsEnabled = true;
            item.Opacity = isUnlocked ? 1.0 : 0.58;
        }
    }

    private void SelectCurrentStepInRail()
    {
        try
        {
            _isUpdatingStepSelection = true;
            StepsListView.SelectedIndex = _currentStepIndex;
        }
        finally
        {
            _isUpdatingStepSelection = false;
        }
    }

    private void SetBusy(bool isBusy, string? message = null)
    {
        BusyProgressRing.IsActive = isBusy;
        BusyProgressRing.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;

        BusyTextBlock.Text = isBusy ? message ?? "Working..." : string.Empty;
        BusyTextBlock.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;

        BackButton.IsEnabled = !isBusy && _currentStepIndex > 0;
        NextButton.IsEnabled = !isBusy && _currentStepIndex < _steps.Count - 1;
        StepsListView.IsEnabled = !isBusy;
        ContentFrame.IsEnabled = !isBusy;
    }

    private string GetBusyMessage(int targetStepIndex)
    {
        return _currentStepIndex switch
        {
            1 when targetStepIndex > _currentStepIndex => "Loading the selected FOLIO CSV...",
            2 when targetStepIndex > _currentStepIndex => "Preparing Bragi subject lists...",
            3 when targetStepIndex > _currentStepIndex => "Building the matched book preview...",
            4 when targetStepIndex > _currentStepIndex => "Preparing the export page...",
            _ => "Working..."
        };
    }

    private void UpdateNavigationButtons()
    {
        BackButton.IsEnabled = _currentStepIndex > 0;
        NextButton.IsEnabled = _currentStepIndex < _steps.Count - 1;

        NextButton.Content = _currentStepIndex < _steps.Count
            ? _steps[_currentStepIndex].PrimaryButtonText
            : "Next";
    }

    private void ShowInfo(string title, string message)
    {
        StatusInfoBar.Severity = InfoBarSeverity.Informational;
        StatusInfoBar.Title = title;
        StatusInfoBar.Message = message;
        StatusInfoBar.IsOpen = true;
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
        string ShortLabel,
        string Description,
        string PrimaryButtonText,
        Type PageType);
}
