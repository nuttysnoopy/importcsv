# CSV Import Application

.NET Core Web API application for importing CSV files into MS SQL Database.

## Features

- Upload CSV files with validation
- Store records in SQL Server database
- Email validation and duplicate checking
- RESTful API for data operations
- Structured error reporting

## Prerequisites

- .NET 8.0 SDK
- SQL Server 2019+ (local or remote)
- Visual Studio Code or Visual Studio

## Setup Instructions

### 1. Database Configuration

Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=CsvImportDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Replace `YOUR_SERVER` with your SQL Server instance name (e.g., `localhost\SQLEXPRESS` for local instance).

### 2. Apply Database Migrations

```bash
dotnet ef database update
```

This will create the database and tables automatically.

### 3. Run the Application

```bash
dotnet run
```

The API will be available at `https://localhost:5001` (or your configured port).

## API Endpoints

### Upload CSV File
- **POST** `/api/csvimport/upload`
- **Parameter:** File (multipart/form-data)
- **Response:** ImportResult with success status and error details

### Get All Records
- **GET** `/api/csvimport/records`
- **Response:** List of CsvRecord objects

### Get Record by ID
- **GET** `/api/csvimport/records/{id}`
- **Response:** Single CsvRecord object

## CSV File Format

The CSV file should have the following columns:

```
Name,Email,Phone,Address
John Doe,john@example.com,123-456-7890,123 Main St
Jane Smith,jane@example.com,098-765-4321,456 Oak Ave
```

- **Name** (Required): Maximum 255 characters
- **Email** (Required): Must be valid email format, Maximum 255 characters
- **Phone** (Optional): Maximum 20 characters
- **Address** (Optional): Maximum 500 characters

## Project Structure

```
CsvImportApp/
├── Models/           # Data models and DTOs
├── Data/             # DbContext and database configuration
├── Services/         # Business logic and CSV processing
├── Controllers/      # API endpoints
├── Migrations/       # Database migrations (auto-generated)
├── Program.cs        # Application startup configuration
└── appsettings.json  # Configuration file
```

## Technologies Used

- **.NET 8.0**: Web framework
- **Entity Framework Core 8.0**: ORM
- **Microsoft.EntityFrameworkCore.SqlServer**: SQL Server provider
- **CsvHelper**: CSV parsing library
- **Serilog**: Logging framework

## Development

### Run in Development Mode
```bash
dotnet run --launch-profile https
```

### Build the Project
```bash
dotnet build
```

### Publish for Production
```bash
dotnet publish -c Release
```

## Notes

- Emails must be unique in the database
- All records are stored with UTC timestamp
- Failed imports provide detailed error messages per row
- Connection strings should use `TrustServerCertificate=True` for local development

## License

This is a template application for CSV import functionality.
