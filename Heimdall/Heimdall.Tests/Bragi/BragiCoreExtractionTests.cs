using Heimdall.BragiCore.Configuration;
using Heimdall.BragiCore.Extraction;
using Heimdall.Domain.Models;

namespace Heimdall.Tests.Bragi;

public sealed class BragiCoreExtractionTests
{
    [Fact]
    public void ExtractFromBookRecords_ExtractsSubjectsFromOfficialCsvBookRecords()
    {
        var books = new[]
        {
            new BookRecord(
                instanceId: "instance-001",
                title: "History Book",
                rawSubjects: "History; Education",
                subjectHeadings: new[]
                {
                    new SubjectHeading("History"),
                    new SubjectHeading("Education")
                })
        };

        var service = new SubjectExtractionService();

        var result = service.ExtractFromBookRecords(books, new BehaviorOptions());

        Assert.Equal(2, result.Subjects.Count);
        Assert.Contains(result.Subjects, subject => subject.OriginalSubject == "History");
        Assert.Contains(result.Subjects, subject => subject.OriginalSubject == "Education");
    }
}
