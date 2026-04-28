namespace Heimdall.Application.Contracts;

public interface ICsvSchemaValidator
{
    void ValidateOrThrow(IReadOnlyCollection<string> columnNames);
}
