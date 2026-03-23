namespace MafiaStore.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string AdditionalImagesJson { get; set; } = "[]";
    public string SizesJson { get; set; } = "[]";
    public int Stock { get; set; }
    public bool Highlight { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
