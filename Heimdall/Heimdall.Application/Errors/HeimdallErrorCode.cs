namespace Heimdall.Application.Errors;

public enum HeimdallErrorCode
{
    Unknown = 0,

    OperationCanceled,

    CsvPathBlank,
    CsvFileNotFound,
    CsvWrongFileType,
    CsvFileUnavailable,
    CsvMissingRequiredColumns,
    CsvEmpty,
    CsvMalformed,

    SubjectListFolderBlank,
    SubjectListFolderNotFound,
    SubjectListFilesMissing,
    SubjectListFilesUnreadable,

    OutputFolderBlank,
    OutputFolderNotFound,
    OutputFolderNotWritable,

    ExportFailed,
    BragiGenerationFailed,
    WorkflowStateInvalid
}
