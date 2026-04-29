using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Heimdall.App.WinUI.Navigation;
using Heimdall.Application.Contracts;
using Heimdall.Application.Workflow;
using Heimdall.Domain.Models;
using Heimdall.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Heimdall.App.WinUI.Pages;

public sealed partial class CategorySelectionPage : Page, IWizardStepPage
{
    private readonly IWorkflowOrchestrator _workflowOrchestrator;
    private readonly WizardSessionStore _sessionStore;
    private readonly List<CategorySelectionItem> _categoryItems = new();

    public CategorySelectionPage()
    {
        InitializeComponent();

        _workflowOrchestrator = App.Services.GetRequiredService<IWorkflowOrchestrator>();
        _sessionStore = App.Services.GetRequiredService<WizardSessionStore>();

        Loaded += CategorySelectionPage_Loaded;
    }

    public async Task<WizardStepResult> OnNextAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<CategoryKey> selectedCategories = GetSelectedCategoryKeys();

        if (selectedCategories.Count == 0)
        {
            ShowError("Select at least one category", "Choose one or more broad categories before building the preview.");
            return WizardStepResult.Failure("Select at least one category", "Choose one or more broad categories before building the preview.");
        }

        try
        {
            _sessionStore.SelectedCategoryKeys = selectedCategories;

            await _workflowOrchestrator.BuildPreviewAsync(
                selectedCategories,
                cancellationToken);

            ShowSuccess("Preview built", $"{selectedCategories.Count} selected category/categories were passed to the workflow.");
            return WizardStepResult.Success();
        }
        catch (Exception ex)
        {
            ShowError("Preview build failed", ex.Message);
            return WizardStepResult.Failure("Preview build failed", ex.Message);
        }
    }

    private void CategorySelectionPage_Loaded(object sender, RoutedEventArgs e)
    {
        LoadCategoriesFromSession();
    }

    private void SelectAllButton_Click(object sender, RoutedEventArgs e)
    {
        SetAllSelections(true);
    }

    private void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        SetAllSelections(false);
    }

    private void LoadCategoriesFromSession()
    {
        CategoryCheckBoxPanel.Children.Clear();
        _categoryItems.Clear();

        if (_sessionStore.SubjectListLoadResult is null ||
            _sessionStore.SubjectListLoadResult.CategorySubjectLists.Count == 0)
        {
            CategoryCountTextBlock.Text = "0 categories loaded";
            ShowError("No categories loaded", "Go back to Subject Source and load or generate Bragi subject lists first.");
            return;
        }

        foreach (CategorySubjectList categorySubjectList in _sessionStore.SubjectListLoadResult.CategorySubjectLists
                     .OrderBy(category => category.Category.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            CategorySelectionItem item = new(
                categorySubjectList.Category,
                categorySubjectList.Subjects.Count);

            _categoryItems.Add(item);

            CheckBox checkBox = new()
            {
                Content = $"{item.DisplayName}    {item.SubjectCount} subjects",
                Tag = item,
                IsChecked = IsPreviouslySelected(item.Category.Key),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            checkBox.Checked += CategoryCheckBox_SelectionChanged;
            checkBox.Unchecked += CategoryCheckBox_SelectionChanged;

            CategoryCheckBoxPanel.Children.Add(checkBox);
        }

        UpdateCategoryCountText();
        HideStatus();
    }

    private bool IsPreviouslySelected(CategoryKey categoryKey)
    {
        return _sessionStore.SelectedCategoryKeys.Any(selected =>
            selected.Value.Equals(categoryKey.Value, StringComparison.OrdinalIgnoreCase));
    }

    private IReadOnlyList<CategoryKey> GetSelectedCategoryKeys()
    {
        return CategoryCheckBoxPanel.Children
            .OfType<CheckBox>()
            .Where(checkBox => checkBox.IsChecked == true)
            .Select(checkBox => (CategorySelectionItem)checkBox.Tag)
            .Select(item => item.Category.Key)
            .ToArray();
    }

    private void SetAllSelections(bool isSelected)
    {
        foreach (CheckBox checkBox in CategoryCheckBoxPanel.Children.OfType<CheckBox>())
        {
            checkBox.IsChecked = isSelected;
        }

        UpdateCategoryCountText();
    }

    private void CategoryCheckBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        UpdateCategoryCountText();
    }

    private void UpdateCategoryCountText()
    {
        int loadedCategoryCount = _categoryItems.Count;
        int selectedCategoryCount = GetSelectedCategoryKeys().Count;
        int totalSubjectCount = _categoryItems.Sum(item => item.SubjectCount);

        CategoryCountTextBlock.Text =
            $"{loadedCategoryCount} categories loaded | {selectedCategoryCount} selected | {totalSubjectCount} total subjects";
    }

    private void ShowSuccess(string title, string message) => ShowStatus(InfoBarSeverity.Success, title, message);

    private void ShowError(string title, string message) => ShowStatus(InfoBarSeverity.Error, title, message);

    private void ShowStatus(InfoBarSeverity severity, string title, string message)
    {
        CategoryStatusInfoBar.Severity = severity;
        CategoryStatusInfoBar.Title = title;
        CategoryStatusInfoBar.Message = message;
        CategoryStatusInfoBar.IsOpen = true;
    }

    private void HideStatus()
    {
        CategoryStatusInfoBar.IsOpen = false;
    }

    private sealed record CategorySelectionItem(
        CategoryDefinition Category,
        int SubjectCount)
    {
        public string DisplayName => Category.DisplayName;
    }
}
