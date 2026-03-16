namespace MafiaStore.Models.ViewModels;

public class CarrinhoItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string ImagemUrl { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Quantidade { get; set; }
    public string Tamanho { get; set; } = string.Empty;
    public decimal Subtotal => Preco * Quantidade;
}
