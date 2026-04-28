namespace Heimdall.Application.Contracts;

public interface ISummaryExtractor
{
    string Extract(string rawNotes);
}
