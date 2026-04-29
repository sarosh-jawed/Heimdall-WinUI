using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Heimdall.App.WinUI.Navigation;
using Heimdall.Application.Contracts;
using Heimdall.Application.Workflow;
using Heimdall.Domain.Models;
using Heimdall.Domain.Results;
using Heimdall.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Heimdall.App.WinUI.Pages;

public sealed partial class PreviewBooksPage : Page, IWizardStepPage
{
    private readonly IWorkflowOrchestrator _workflowOrchestrator;
    private readonly WizardSessionStore _sessionStore;
    private EmailPreviewResult? _previewResult;
    private CategoryKey? _selectedCategoryKey;
    private bool _selectedCannotSort;

    public PreviewBooksPage()
    {
        InitializeComponent();

        _workflowOrchestrator = App.Services.GetRequiredService<IWorkflowOrchestrator>();
        _sessionStore = App.Services.GetRequiredService<WizardSessionStore>();

        Loaded += PreviewBooksPage_Loaded;
    }

    public Task<WizardStepResult> OnNextAsync(CancellationToken cancellationToken)
    {
        if (_sessionStore.PreviewResult is null)
        {
            ShowError("Preview required", "Build the category preview before continuing to export.");
            return Task.FromResult(WizardStepResult.Failure(
                "Preview required",
                "Build the category preview before continuing to export."));
        }

        return Task.FromResult(WizardStepResult.Success());
    }

    private void PreviewBooksPage_Loaded(object sender, RoutedEventArgs e)
    {
        LoadPreview();
    }

    private void LoadPreview()
    {
        _previewResult = _sessionStore.PreviewResult;

        if (_previewResult is null)
        {
            CategoryListView.Items.Clear();
            BookCardsPanel.Children.Clear();
            PreviewSummaryTextBlock.Text = "No preview is available.";
            BookPreviewHeaderTextBlock.Text = "Book preview";
            BookPreviewCountTextBlock.Text = "Go back to Select Categories and build a preview first.";
            ShowError("No preview found", "Go back to Select Categories and build the preview before reviewing books.");
            return;
        }

        RefreshCategoryList();
        HideStatus();
    }

    private void RefreshCategoryList(CategoryKey? categoryToSelect = null, bool selectCannotSort = false)
    {
        if (_previewResult is null)
        {
            return;
        }

        CategoryListView.SelectionChanged -= CategoryListView_SelectionChanged;
        CategoryListView.Items.Clear();

        int totalMatchedBooks = _previewResult.Categories.Sum(category => category.Books.Count);
        int totalActiveBooks = _previewResult.Categories.Sum(category => category.ActiveBookCount);
        int totalRemovedBooks = totalMatchedBooks - totalActiveBooks;

        PreviewSummaryTextBlock.Text =
            $"{_previewResult.Categories.Count} categories | {totalActiveBooks} active books | {totalRemovedBooks} removed | {_previewResult.CannotSortBooks.Count} CannotSort";

        List<PreviewCategoryDisplayItem> items = new();

        foreach (EmailCategoryPreview categoryPreview in _previewResult.Categories
                     .OrderBy(category => category.Category.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            items.Add(new PreviewCategoryDisplayItem(
                categoryPreview.Category.Key,
                categoryPreview.Category.DisplayName,
                categoryPreview.ActiveBookCount,
                categoryPreview.Books.Count,
                false));
        }

        if (_previewResult.CannotSortBooks.Count > 0)
        {
            items.Add(new PreviewCategoryDisplayItem(
                null,
                "CannotSort",
                _previewResult.CannotSortBooks.Count,
                _previewResult.CannotSortBooks.Count,
                true));
        }

        foreach (PreviewCategoryDisplayItem item in items)
        {
            CategoryListView.Items.Add(item);
        }

        CategoryListView.SelectionChanged += CategoryListView_SelectionChanged;

        int selectedIndex = FindSelectedIndex(items, categoryToSelect, selectCannotSort);

        if (selectedIndex >= 0)
        {
            CategoryListView.SelectedIndex = selectedIndex;
            RenderSelectedCategory(items[selectedIndex]);
        }
        else if (items.Count > 0)
        {
            CategoryListView.SelectedIndex = 0;
            RenderSelectedCategory(items[0]);
        }
        else
        {
            RenderEmptyPreview();
        }
    }

    private int FindSelectedIndex(
        IReadOnlyList<PreviewCategoryDisplayItem> items,
        CategoryKey? categoryToSelect,
        bool selectCannotSort)
    {
        if (selectCannotSort)
        {
            return items
                .Select((item, index) => new { item, index })
                .FirstOrDefault(pair => pair.item.IsCannotSort)
                ?.index ?? -1;
        }

        if (categoryToSelect is not null)
        {
            return items
                .Select((item, index) => new { item, index })
                .FirstOrDefault(pair =>
                    pair.item.CategoryKey is not null &&
                    pair.item.CategoryKey.Value.Equals(categoryToSelect.Value, StringComparison.OrdinalIgnoreCase))
                ?.index ?? -1;
        }

        if (_selectedCannotSort)
        {
            return items
                .Select((item, index) => new { item, index })
                .FirstOrDefault(pair => pair.item.IsCannotSort)
                ?.index ?? -1;
        }

        if (_selectedCategoryKey is not null)
        {
            return items
                .Select((item, index) => new { item, index })
                .FirstOrDefault(pair =>
                    pair.item.CategoryKey is not null &&
                    pair.item.CategoryKey.Value.Equals(_selectedCategoryKey.Value, StringComparison.OrdinalIgnoreCase))
                ?.index ?? -1;
        }

        return -1;
    }

    private void CategoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryListView.SelectedItem is PreviewCategoryDisplayItem selectedItem)
        {
            RenderSelectedCategory(selectedItem);
        }
    }

    private void RenderSelectedCategory(PreviewCategoryDisplayItem selectedItem)
    {
        if (_previewResult is null)
        {
            return;
        }

        _selectedCategoryKey = selectedItem.CategoryKey;
        _selectedCannotSort = selectedItem.IsCannotSort;

        if (selectedItem.IsCannotSort)
        {
            RenderCannotSortPreview();
            return;
        }

        EmailCategoryPreview? categoryPreview = _previewResult.Categories.FirstOrDefault(category =>
            selectedItem.CategoryKey is not null &&
            category.Category.Key.Value.Equals(selectedItem.CategoryKey.Value, StringComparison.OrdinalIgnoreCase));

        if (categoryPreview is null)
        {
            RenderEmptyPreview();
            return;
        }

        RenderCategoryPreview(categoryPreview);
    }

    private void RenderCategoryPreview(EmailCategoryPreview categoryPreview)
    {
        BookCardsPanel.Children.Clear();

        IReadOnlyList<EmailBookItem> activeBooks = categoryPreview.ActiveBooks;
        int removedCount = categoryPreview.Books.Count - activeBooks.Count;

        BookPreviewHeaderTextBlock.Text = $"{categoryPreview.Category.DisplayName} preview";
        BookPreviewCountTextBlock.Text =
            $"{activeBooks.Count} active of {categoryPreview.Books.Count} matched books. {removedCount} removed from this category.";

        BookPreviewInfoBar.IsOpen = true;
        BookPreviewInfoBar.Message = "Removing a book here removes it only from this category's final email file.";

        if (activeBooks.Count == 0)
        {
            BookCardsPanel.Children.Add(CreateEmptyTextBlock("No active books remain in this category."));
            return;
        }

        foreach (EmailBookItem book in activeBooks.OrderBy(book => book.Title, StringComparer.OrdinalIgnoreCase))
        {
            BookCardsPanel.Children.Add(CreateBookCard(book, categoryPreview.Category.Key, canRemove: true));
        }
    }

    private void RenderCannotSortPreview()
    {
        if (_previewResult is null)
        {
            return;
        }

        BookCardsPanel.Children.Clear();

        BookPreviewHeaderTextBlock.Text = "CannotSort preview";
        BookPreviewCountTextBlock.Text =
            $"{_previewResult.CannotSortBooks.Count} book(s) did not match any selected category.";

        BookPreviewInfoBar.IsOpen = true;
        BookPreviewInfoBar.Message = "CannotSort books are summarized here and will be exported separately when unmatched records exist.";

        if (_previewResult.CannotSortBooks.Count == 0)
        {
            BookCardsPanel.Children.Add(CreateEmptyTextBlock("No CannotSort books are present."));
            return;
        }

        foreach (EmailBookItem book in _previewResult.CannotSortBooks.OrderBy(book => book.Title, StringComparer.OrdinalIgnoreCase))
        {
            BookCardsPanel.Children.Add(CreateBookCard(book, categoryKey: null, canRemove: false));
        }
    }

    private void RenderEmptyPreview()
    {
        BookCardsPanel.Children.Clear();
        BookPreviewHeaderTextBlock.Text = "Book preview";
        BookPreviewCountTextBlock.Text = "No preview categories are available.";
        BookPreviewInfoBar.IsOpen = false;
        BookCardsPanel.Children.Add(CreateEmptyTextBlock("No books are available to preview."));
    }

    private Border CreateBookCard(
        EmailBookItem book,
        CategoryKey? categoryKey,
        bool canRemove)
    {
        StackPanel contentPanel = new()
        {
            Spacing = 8
        };

        contentPanel.Children.Add(new TextBlock
        {
            Text = book.Title,
            FontSize = 16,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });

        contentPanel.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(book.Author)
                ? "Author not provided"
                : book.Author,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["TextFillColorSecondaryBrush"],
            TextWrapping = TextWrapping.Wrap
        });

        contentPanel.Children.Add(new TextBlock
        {
            Text = BuildSummaryPreview(book.Summary),
            TextWrapping = TextWrapping.Wrap
        });

        if (canRemove && categoryKey is not null)
        {
            Button removeButton = new()
            {
                Content = "Remove from this category",
                HorizontalAlignment = HorizontalAlignment.Left,
                Tag = new RemoveBookRequest(categoryKey, book.BookId)
            };

            removeButton.Click += RemoveBookButton_Click;

            contentPanel.Children.Add(removeButton);
        }
        else
        {
            contentPanel.Children.Add(new TextBlock
            {
                Text = "CannotSort records are not removed here. They are exported separately for review.",
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["TextFillColorSecondaryBrush"],
                TextWrapping = TextWrapping.Wrap
            });
        }

        return new Border
        {
            Padding = new Thickness(14),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["CardStrokeColorDefaultBrush"],
            Background = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["CardBackgroundFillColorDefaultBrush"],
            Child = contentPanel
        };
    }

    private async void RemoveBookButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button ||
            button.Tag is not RemoveBookRequest request)
        {
            return;
        }

        try
        {
            button.IsEnabled = false;

            _previewResult = await _workflowOrchestrator.RemoveBookFromPreviewAsync(
                request.CategoryKey,
                request.BookId);

            ShowSuccess("Book removed", "The book was removed only from the selected category preview.");
            RefreshCategoryList(request.CategoryKey);
        }
        catch (Exception ex)
        {
            ShowError("Remove failed", ex.Message);
            button.IsEnabled = true;
        }
    }

    private static TextBlock CreateEmptyTextBlock(string message)
    {
        return new TextBlock
        {
            Text = message,
            Foreground = (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["TextFillColorSecondaryBrush"],
            TextWrapping = TextWrapping.Wrap
        };
    }

    private static string BuildSummaryPreview(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return "No summary available.";
        }

        const int maxLength = 320;

        string cleaned = summary.Trim();

        return cleaned.Length <= maxLength
            ? cleaned
            : cleaned[..maxLength] + "...";
    }

    private void ShowSuccess(string title, string message)
    {
        ShowStatus(InfoBarSeverity.Success, title, message);
    }

    private void ShowError(string title, string message)
    {
        ShowStatus(InfoBarSeverity.Error, title, message);
    }

    private void HideStatus()
    {
        PreviewStatusInfoBar.IsOpen = false;
    }

    private void ShowStatus(InfoBarSeverity severity, string title, string message)
    {
        PreviewStatusInfoBar.Severity = severity;
        PreviewStatusInfoBar.Title = title;
        PreviewStatusInfoBar.Message = message;
        PreviewStatusInfoBar.IsOpen = true;
    }

    private sealed record PreviewCategoryDisplayItem(
        CategoryKey? CategoryKey,
        string DisplayName,
        int ActiveBookCount,
        int TotalBookCount,
        bool IsCannotSort)
    {
        public override string ToString()
        {
            return IsCannotSort
                ? $"CannotSort ({TotalBookCount})"
                : $"{DisplayName} ({ActiveBookCount}/{TotalBookCount})";
        }
    }

    private sealed record RemoveBookRequest(
        CategoryKey CategoryKey,
        string BookId);
}

