using MafiaStore.Models.ViewModels;

namespace MafiaStore.Services;

public interface IProductCatalogService
{
    IReadOnlyList<ProdutoViewModel> GetAll();
    ProdutoViewModel? GetById(int id);
    int GetNextId();
    bool Create(ProdutoViewModel produto, out string error);
    bool Update(ProdutoViewModel produto, out string error);
    bool Delete(int id);
}
