namespace CsvImportApp.Models;

public class CsvRecordDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class ImportResult
{
    public bool Success { get; set; }
    public int RecordsImported { get; set; }
    public int RecordsFailed { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}
