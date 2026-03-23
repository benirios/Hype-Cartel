using MafiaStore.Models.ViewModels;

namespace MafiaStore.Services;

public interface ICartStore
{
    List<CarrinhoItemViewModel> GetItems(string ownerKey);
    int GetCartCount(string ownerKey);
    bool AddItem(string ownerKey, int produtoId, string? tamanho);
    bool UpdateQuantity(string ownerKey, int produtoId, string? tamanho, int quantidade);
    bool RemoveItem(string ownerKey, int produtoId, string? tamanho);
    bool Clear(string ownerKey);
}
