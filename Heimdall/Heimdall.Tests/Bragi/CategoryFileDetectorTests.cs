using Heimdall.BragiCore.Configuration;
using Heimdall.Infrastructure.Bragi;
using Heimdall.Application.Errors;

namespace Heimdall.Tests.Bragi;

public sealed class CategoryFileDetectorTests
{
    [Fact]
    public void Detect_DetectsCategoriesFromBragiSubjectFiles()
    {
        var folder = CreateTempFolder();

        try
        {
            File.WriteAllText(Path.Combine(folder, "ArtSubjects.txt"), "Art");
            File.WriteAllText(Path.Combine(folder, "ComputerSubjects.txt"), "Computer science");
            File.WriteAllText(Path.Combine(folder, "SlimSubjects.txt"), "Libraries");

            var detector = new CategoryFileDetector(new BragiCoreOptions());

            var result = detector.Detect(folder);

            Assert.Contains(result.Categories, category => category.DisplayName == "Art");
            Assert.Contains(result.Categories, category => category.DisplayName == "Computer");
            Assert.Contains(result.Categories, category => category.DisplayName == "SLIM");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void Detect_ThrowsFriendlyError_WhenFolderIsMissing()
    {
        var missingFolder = Path.Combine(Path.GetTempPath(), $"heimdall-missing-{Guid.NewGuid():N}");
        var detector = new CategoryFileDetector(new BragiCoreOptions());

        var exception = Assert.Throws<UserFriendlyException>(() => detector.Detect(missingFolder));

        Assert.Equal(HeimdallErrorCode.SubjectListFolderNotFound, exception.ErrorCode);
        Assert.Equal("Existing Bragi folder not found", exception.Title);
        Assert.Contains("could not be found", exception.FullUserMessage);
    }

    [Fact]
    public void Detect_ThrowsFriendlyError_WhenFolderHasNoSubjectFiles()
    {
        var folder = CreateTempFolder();

        try
        {
            var detector = new CategoryFileDetector(new BragiCoreOptions());

            var exception = Assert.Throws<UserFriendlyException>(() => detector.Detect(folder));

            Assert.Equal(HeimdallErrorCode.SubjectListFilesMissing, exception.ErrorCode);
            Assert.Equal("No Bragi subject files found", exception.Title);
            Assert.Contains("No Bragi subject-list files were found", exception.FullUserMessage);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public void Detect_AddsWarning_WhenSomeExpectedFilesAreMissing()
    {
        var folder = CreateTempFolder();

        try
        {
            File.WriteAllText(Path.Combine(folder, "ArtSubjects.txt"), "Art");

            var detector = new CategoryFileDetector(new BragiCoreOptions());

            var result = detector.Detect(folder);

            Assert.NotEmpty(result.Warnings);
            Assert.Contains("Some expected Bragi subject-list files were not found", result.Warnings[0]);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"heimdall-category-detector-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
