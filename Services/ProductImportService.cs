
using CsvImportApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductImportApp.Services;

public interface IProductImportService
{
    Task<ImportResult> ImportCsvAsync(Stream fileStream, string fileName);
    Task<List<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
}

public class ProductImportService : IProductImportService
{
    private readonly CsvImportApp.Data.CsvImportDbContext _dbContext;
    private readonly ILogger<ProductImportService> _logger;

    public ProductImportService(CsvImportApp.Data.CsvImportDbContext dbContext, ILogger<ProductImportService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ImportResult> ImportCsvAsync(Stream fileStream, string fileName)
    {
        var result = new ImportResult { Success = true };

        try
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                result.Success = false;
                result.Message = "File is empty";
                return result;
            }

            var records = new List<Product>();
            var errors = new List<string>();

            using (var reader = new StreamReader(fileStream))
            using (var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<ProductMap>();
                var recordList = csv.GetRecords<ProductDto>().ToList();

                // ดึงข้อมูลทั้งหมดจาก database มาเก็บใน memory ก่อน
                var allProductCodes = recordList.Select(r => r.ProductCode).Distinct().ToList();
                var existingProducts = await _dbContext.Products
                    .Where(r => allProductCodes.Contains(r.ProductCode))
                    .ToDictionaryAsync(r => r.ProductCode, r => r);

                int rowNumber = 2;

                foreach (var record in recordList)
                {
                    try
                    {
                        // Validate record
                        var validationError = ValidateRecord(record);
                        if (!string.IsNullOrEmpty(validationError))
                        {
                            errors.Add($"Row {rowNumber}: {validationError}");
                            result.RecordsFailed++;
                            rowNumber++;
                            continue;
                        }

                        // เช็คว่า Product มีอยู่แล้วหรือไม่
                        if (existingProducts.TryGetValue(record.ProductCode, out var existingRecord))
                        {
                            // ⭐ เช็คว่ามีการเปลี่ยนแปลงจริงหรือไม่
                            bool hasChanges = false;

                            if (existingRecord.ProductCode != record.ProductCode)
                            {
                                existingRecord.ProductCode = record.ProductCode;
                                hasChanges = true;
                            }

                            /*if (existingRecord.Name != record.Name)
                            {
                                existingRecord.Name = record.Name;
                                hasChanges = true;
                            }

                            if (existingRecord.Description != record.Description)
                            {
                                existingRecord.Description = record.Description;
                                hasChanges = true;
                            }

                            if (existingRecord.Price != record.Price)
                            {
                                existingRecord.Price = record.Price;
                                hasChanges = true;
                            }

                            if (existingRecord.Cost != record.Cost)
                            {
                                existingRecord.Cost = record.Cost;
                                hasChanges = true;
                            }*/

                            if (existingRecord.Stock != record.Stock)
                            {
                                existingRecord.Stock = record.Stock;
                                hasChanges = true;
                            }

                            if (existingRecord.MinStock != record.MinStock)
                            {
                                existingRecord.MinStock = record.MinStock;
                                hasChanges = true;
                            }

                            /*if (existingRecord.ImageUrl != record.ImageUrl)
                            {
                                existingRecord.ImageUrl = record.ImageUrl;
                                hasChanges = true;
                            }*/


                            // ⭐ อัปเดตเฉพาะเมื่อมีการเปลี่ยนแปลงจริงๆ
                            if (hasChanges)
                            {
                                existingRecord.UpdatedAt = DateTime.UtcNow;
                                result.RecordsUpdated++;
                            }
                            else
                            {
                                // ไม่มีการเปลี่ยนแปลง - ไม่ต้องทำอะไร
                                // EF Core จะไม่ generate UPDATE statement
                            }
                        }
                        else
                        {
                            // สร้าง record ใหม่
                            var product = new Product
                            {
                                ProductCode = record.ProductCode,
                                Name = record.Name,
                                Description = record.Description,
                                Price = record.Price,
                                Cost = record.Cost,
                                Stock = record.Stock,
                                MinStock = record.MinStock,
                                ImageUrl = record.ImageUrl,
                                CreatedAt = DateTime.UtcNow
                            };

                            _dbContext.Products.Add(product);
                            result.RecordsImported++;
                        }

                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Row {rowNumber}: {ex.Message}");
                        result.RecordsFailed++;
                    }

                    rowNumber++;
                }
            }

            // Save to database
            if (records.Count > 0)
            {
                await _dbContext.Products.AddRangeAsync(records);
            }

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"Successfully imported {result.RecordsImported} new records and updated {result.RecordsUpdated} records from {fileName}");

            result.Message = $"Import completed: {result.RecordsImported} new, {result.RecordsUpdated} updated, {result.RecordsFailed} failed";
            result.Errors = errors;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error importing CSV: {ex.Message}");
            result.Success = false;
            result.Message = $"Error importing CSV: {ex.Message}";
            result.Errors.Add(ex.Message);
            return result;
        }
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _dbContext.Products.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        return await _dbContext.Products.FindAsync(id);
    }

    private string ValidateRecord(ProductDto record)
    {
        if (string.IsNullOrWhiteSpace(record.ProductCode))
            return "ProductCode is required";

        return string.Empty;
    }

}
public class ProductMap : CsvHelper.Configuration.ClassMap<ProductDto>
{
    public ProductMap()
    {
        Map(m => m.ProductCode).Name("ProductCode");
        Map(m => m.Name).Name("Name");
        Map(m => m.Description).Name("Description").Optional();
        Map(m => m.Price).Name("Price");
        Map(m => m.Cost).Name("Cost");
        Map(m => m.Stock).Name("Stock");
        Map(m => m.MinStock).Name("MinStock");
        Map(m => m.ImageUrl).Name("ImageUrl").Optional();
    }
}
