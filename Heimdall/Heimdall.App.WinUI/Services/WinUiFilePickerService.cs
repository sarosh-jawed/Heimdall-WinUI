using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Heimdall.App.WinUI.Services;

public sealed class WinUiFilePickerService : IFilePickerService
{
    public async Task<string?> PickCsvFileAsync()
    {
        FileOpenPicker picker = new()
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary
        };

        picker.FileTypeFilter.Add(".csv");

        InitializePickerWithMainWindow(picker);

        StorageFile? file = await picker.PickSingleFileAsync();

        if (file is null)
        {
            return null;
        }

        if (!file.FileType.Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Please select a valid .csv file.");
        }

        return file.Path;
    }

    public Task<string?> PickOutputFolderAsync()
    {
        return PickFolderAsync(PickerLocationId.DocumentsLibrary);
    }

    public Task<string?> PickExistingBragiFolderAsync()
    {
        return PickFolderAsync(PickerLocationId.DocumentsLibrary);
    }

    private static async Task<string?> PickFolderAsync(PickerLocationId suggestedStartLocation)
    {
        FolderPicker picker = new()
        {
            SuggestedStartLocation = suggestedStartLocation
        };

        // WinUI folder pickers require at least one file type filter, even though folders are being selected.
        picker.FileTypeFilter.Add("*");

        InitializePickerWithMainWindow(picker);

        StorageFolder? folder = await picker.PickSingleFolderAsync();

        return folder?.Path;
    }

    private static void InitializePickerWithMainWindow(object picker)
    {
        IntPtr windowHandle = MainWindow.WindowHandle;

        if (windowHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException("The Heimdall window is not ready for file selection yet.");
        }

        WinRT.Interop.InitializeWithWindow.Initialize(picker, windowHandle);
    }
}
