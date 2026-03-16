namespace MafiaStore.Models.ViewModels;

public class ProdutoViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public string ImagemUrl { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Destaque { get; set; }
    public List<string> Tamanhos { get; set; } = new();
    public List<string> ImagensAdicionais { get; set; } = new();
}
