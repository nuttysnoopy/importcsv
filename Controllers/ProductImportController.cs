using Microsoft.AspNetCore.Mvc;
using ProductImportApp.Services;

namespace CsvImportApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductImportController : ControllerBase
{
    private readonly ProductImportService _productImportService;
    private readonly ILogger<ProductImportController> _logger;

    public ProductImportController(ProductImportService productImportService, ILogger<ProductImportController> logger)
    {
        _productImportService = productImportService;
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
                var result = await _productImportService.ImportCsvAsync(stream, file.FileName);
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
            var records = await _productImportService.GetAllProductsAsync();
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
            var record = await _productImportService.GetProductByIdAsync(id);
            if (record == null)
            {
                return NotFound(new { message = "Product not found" });
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
