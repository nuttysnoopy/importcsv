using CsvImportApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CsvImportApp.Services;

public interface ICsvImportService
{
    Task<ImportResult> ImportCsvAsync(Stream fileStream, string fileName);
    Task<List<CsvRecord>> GetAllRecordsAsync();
    Task<CsvRecord?> GetRecordByIdAsync(int id);
}

public class CsvImportService : ICsvImportService
{
    private readonly CsvImportApp.Data.CsvImportDbContext _dbContext;
    private readonly ILogger<CsvImportService> _logger;

    public CsvImportService(CsvImportApp.Data.CsvImportDbContext dbContext, ILogger<CsvImportService> logger)
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

            var records = new List<CsvRecord>();
            var errors = new List<string>();

            using (var reader = new StreamReader(fileStream))
            using (var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<CsvRecordMap>();
                var recordList = csv.GetRecords<CsvRecordDto>().ToList();

                // ดึงข้อมูลทั้งหมดจาก database มาเก็บใน memory ก่อน
                var allEmails = recordList.Select(r => r.Email).Distinct().ToList();
                var existingRecords = await _dbContext.CsvRecords
                    .Where(r => allEmails.Contains(r.Email))
                    .ToDictionaryAsync(r => r.Email, r => r);

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

                        // เช็คว่า email มีอยู่แล้วหรือไม่
                        if (existingRecords.TryGetValue(record.Email, out var existingRecord))
                        {
                            // ⭐ เช็คว่ามีการเปลี่ยนแปลงจริงหรือไม่
                            bool hasChanges = false;

                            if (existingRecord.Phone != record.Phone)
                            {
                                existingRecord.Phone = record.Phone;
                                hasChanges = true;
                            }

                            if (existingRecord.Address != record.Address)
                            {
                                existingRecord.Address = record.Address;
                                hasChanges = true;
                            }

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
                            var csvRecord = new CsvRecord
                            {
                                Name = record.Name,
                                Email = record.Email,
                                Phone = record.Phone,
                                Address = record.Address,
                                CreatedAt = DateTime.UtcNow
                            };

                            records.Add(csvRecord);
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
                await _dbContext.CsvRecords.AddRangeAsync(records);
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

    public async Task<List<CsvRecord>> GetAllRecordsAsync()
    {
        return await _dbContext.CsvRecords.OrderByDescending(r => r.CreatedAt).ToListAsync();
    }

    public async Task<CsvRecord?> GetRecordByIdAsync(int id)
    {
        return await _dbContext.CsvRecords.FindAsync(id);
    }

    private string ValidateRecord(CsvRecordDto record)
    {
        if (string.IsNullOrWhiteSpace(record.Name))
            return "Name is required";

        if (string.IsNullOrWhiteSpace(record.Email))
            return "Email is required";

        if (!IsValidEmail(record.Email))
            return "Email format is invalid";

        if (record.Name.Length > 255)
            return "Name is too long (max 255 characters)";

        if (record.Email.Length > 255)
            return "Email is too long (max 255 characters)";

        return string.Empty;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

public class CsvRecordMap : CsvHelper.Configuration.ClassMap<CsvRecordDto>
{
    public CsvRecordMap()
    {
        Map(m => m.Name).Name("Name");
        Map(m => m.Email).Name("Email");
        Map(m => m.Phone).Name("Phone").Optional();
        Map(m => m.Address).Name("Address").Optional();
    }
}
