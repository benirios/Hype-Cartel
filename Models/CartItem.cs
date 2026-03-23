namespace MafiaStore.Models;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public Cart? Cart { get; set; }

    public int ProductId { get; set; }
    public string Size { get; set; } = "M";
    public int Quantity { get; set; } = 1;
}
