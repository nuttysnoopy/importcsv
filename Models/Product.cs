namespace CsvImportApp.Models;

public class Product
{
    public int Id { get; set; }
    
    public string ProductCode { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    
    public decimal Cost { get; set; }
    
    public int Stock { get; set; }
    
    public int MinStock { get; set; }
    
    public string? ImageUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Foreign Keys
    /*public int CategoryId { get; set; }
    
    public int? SupplierId { get; set; }
    
    // Navigation Properties
    public Category? Category { get; set; }
    
    public Supplier? Supplier { get; set; }*/
}