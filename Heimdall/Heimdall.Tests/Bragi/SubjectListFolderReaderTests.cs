using Heimdall.BragiCore.Configuration;
using Heimdall.Infrastructure.Bragi;

namespace Heimdall.Tests.Bragi;

public sealed class SubjectListFolderReaderTests
{
    [Fact]
    public async Task ReadAsync_ReadsValidBragiOutputFolder()
    {
        var folder = CreateTempFolder();

        try
        {
            File.WriteAllText(Path.Combine(folder, "ArtSubjects.txt"), "Painting\r\nArtists\r\n");

            var reader = CreateReader();

            var result = await reader.ReadAsync(folder);

            Assert.Contains(result.CategorySubjectLists, list => list.Category.DisplayName == "Art");
            Assert.Contains(
                result.CategorySubjectLists.Single(list => list.Category.DisplayName == "Art").Subjects,
                subject => subject.RawValue == "Painting");
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task ReadAsync_HandlesCrLfAndLfLineEndings()
    {
        var folder = CreateTempFolder();

        try
        {
            File.WriteAllText(Path.Combine(folder, "ArtSubjects.txt"), "Painting\r\nArtists\nSculpture\r\n");

            var reader = CreateReader();

            var result = await reader.ReadAsync(folder);
            var artSubjects = result.CategorySubjectLists
                .Single(list => list.Category.DisplayName == "Art")
                .Subjects
                .Select(subject => subject.RawValue)
                .ToArray();

            Assert.Contains("Painting", artSubjects);
            Assert.Contains("Artists", artSubjects);
            Assert.Contains("Sculpture", artSubjects);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task ReadAsync_TrimsAndDeduplicatesSubjectValues()
    {
        var folder = CreateTempFolder();

        try
        {
            File.WriteAllText(
                Path.Combine(folder, "ComputerSubjects.txt"),
                "  Artificial intelligence  \r\nartificial intelligence\r\nComputer science\r\n");

            var reader = CreateReader();

            var result = await reader.ReadAsync(folder);
            var computerSubjects = result.CategorySubjectLists
                .Single(list => list.Category.DisplayName == "Computer")
                .Subjects
                .Select(subject => subject.RawValue)
                .ToArray();

            Assert.Equal(2, computerSubjects.Length);
            Assert.Contains("Artificial intelligence", computerSubjects);
            Assert.Contains("Computer science", computerSubjects);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task ReadAsync_ThrowsFriendlyError_WhenFolderIsMissing()
    {
        var missingFolder = Path.Combine(Path.GetTempPath(), $"heimdall-missing-folder-{Guid.NewGuid():N}");
        var reader = CreateReader();

        var exception = await Assert.ThrowsAsync<DirectoryNotFoundException>(() => reader.ReadAsync(missingFolder));

        Assert.Contains("selected Bragi subject-list folder was not found", exception.Message);
    }

    [Fact]
    public async Task ReadAsync_ThrowsFriendlyError_WhenFolderIsEmpty()
    {
        var folder = CreateTempFolder();

        try
        {
            var reader = CreateReader();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => reader.ReadAsync(folder));

            Assert.Equal("No Bragi subject-list files were found in the selected folder.", exception.Message);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    [Fact]
    public async Task ReadAsync_AllowsContinuing_WhenSomeExpectedFilesAreMissing()
    {
        var folder = CreateTempFolder();

        try
        {
            File.WriteAllText(Path.Combine(folder, "SlimSubjects.txt"), "Libraries\r\n");

            var reader = CreateReader();

            var result = await reader.ReadAsync(folder);

            Assert.Single(result.CategorySubjectLists);
            Assert.Equal("SLIM", result.CategorySubjectLists[0].Category.DisplayName);
            Assert.NotEmpty(result.Warnings);
        }
        finally
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private static SubjectListFolderReader CreateReader()
    {
        var detector = new CategoryFileDetector(new BragiCoreOptions());

        return new SubjectListFolderReader(detector);
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"heimdall-subject-reader-{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }
}
