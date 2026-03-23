namespace MafiaStore.Models;

public class Cart
{
    public int Id { get; set; }
    public string OwnerKey { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<CartItem> Items { get; set; } = new();
}
