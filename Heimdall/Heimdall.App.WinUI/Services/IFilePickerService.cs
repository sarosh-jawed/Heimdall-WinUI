using System.Threading.Tasks;

namespace Heimdall.App.WinUI.Services;

public interface IFilePickerService
{
    Task<string?> PickCsvFileAsync();
    Task<string?> PickOutputFolderAsync();
    Task<string?> PickExistingBragiFolderAsync();
}
