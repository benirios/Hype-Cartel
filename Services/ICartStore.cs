using MafiaStore.Models.ViewModels;

namespace MafiaStore.Services;

public interface ICartStore
{
    List<CarrinhoItemViewModel> GetItems();
    int GetCartCount();
    bool AddItem(int produtoId, string? tamanho);
    bool UpdateQuantity(int produtoId, string? tamanho, int quantidade);
    bool RemoveItem(int produtoId, string? tamanho);
}
