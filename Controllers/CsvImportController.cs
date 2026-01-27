using CsvImportApp.Models;
using CsvImportApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace CsvImportApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CsvImportController : ControllerBase
{
    private readonly ICsvImportService _csvImportService;
    private readonly ILogger<CsvImportController> _logger;

    public CsvImportController(ICsvImportService csvImportService, ILogger<CsvImportController> logger)
    {
        _csvImportService = csvImportService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Only CSV files are allowed" });
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var result = await _csvImportService.ImportCsvAsync(stream, file.FileName);
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading file: {ex.Message}");
            return StatusCode(500, new { message = $"Error uploading file: {ex.Message}" });
        }
    }

    [HttpGet("records")]
    public async Task<IActionResult> GetAllRecords()
    {
        try
        {
            var records = await _csvImportService.GetAllRecordsAsync();
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving records: {ex.Message}");
            return StatusCode(500, new { message = $"Error retrieving records: {ex.Message}" });
        }
    }

    [HttpGet("records/{id}")]
    public async Task<IActionResult> GetRecord(int id)
    {
        try
        {
            var record = await _csvImportService.GetRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound(new { message = "Record not found" });
            }

            return Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving record: {ex.Message}");
            return StatusCode(500, new { message = $"Error retrieving record: {ex.Message}" });
        }
    }
}
