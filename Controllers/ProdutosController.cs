using Microsoft.AspNetCore.Mvc;
using MafiaStore.Models.ViewModels;

namespace MafiaStore.Controllers;

public class ProdutosController : Controller
{
    private static List<ProdutoViewModel> GetProdutos()
    {
        return new List<ProdutoViewModel>
        {
            new()
            {
                Id = 1,
                Nome = "Shadow Overcoat",
                Slug = "shadow-overcoat",
                Preco = 349m,
                ImagemUrl = "/catalog/shadow-overcoat.svg",
                Categoria = "Outerwear",
                Descricao = "A floor-grazing silhouette cut from heavyweight wool-blend cloth. The Shadow Overcoat drapes the body in architectural darkness — raw edges, concealed closures, and a presence that enters the room before you do.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 2,
                Nome = "Phantom Bomber",
                Slug = "phantom-bomber",
                Preco = 289m,
                ImagemUrl = "/catalog/phantom-bomber.svg",
                Categoria = "Outerwear",
                Descricao = "Matte nylon shell with a washed silk lining. The Phantom Bomber is built for those who vanish into the night and reappear at dawn — minimal hardware, maximum intent, ribbed cuffs that grip like loyalty.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 3,
                Nome = "Noir Trench",
                Slug = "noir-trench",
                Preco = 389m,
                ImagemUrl = "/catalog/noir-trench.svg",
                Categoria = "Outerwear",
                Descricao = "Double-breasted, belt-cinched, and cut from water-resistant cotton twill. The Noir Trench is a monument to restraint — storm flaps that whisper authority, a collar that frames the jaw like a verdict.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 4,
                Nome = "Obsidian Parka",
                Slug = "obsidian-parka",
                Preco = 329m,
                ImagemUrl = "/catalog/obsidian-parka.svg",
                Categoria = "Outerwear",
                Descricao = "Insulated with ethically-sourced down and sheathed in waxed cotton. The Obsidian Parka carries weight without burden — deep pockets for secrets, a hood that frames the face in shadow.",
                Destaque = false,
                Tamanhos = new List<string> { "S", "M", "L", "XL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 5,
                Nome = "Silk Noir Shirt",
                Slug = "silk-noir-shirt",
                Preco = 189m,
                ImagemUrl = "/catalog/silk-noir-shirt.svg",
                Categoria = "Shirts",
                Descricao = "Pure mulberry silk, dyed to the depth of a moonless sky. The Silk Noir Shirt falls on the body like a second conscience — effortless drape, hidden placket, French seams that respect the craft.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 6,
                Nome = "Ghost Linen Tee",
                Slug = "ghost-linen-tee",
                Preco = 89m,
                ImagemUrl = "/catalog/ghost-linen-tee.svg",
                Categoria = "Shirts",
                Descricao = "Washed Belgian linen with a raw-cut hem. The Ghost Linen Tee is the foundation layer — lived-in softness from the first wear, a relaxed drop shoulder that moves like smoke through still air.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 7,
                Nome = "Cartel Oxford",
                Slug = "cartel-oxford",
                Preco = 149m,
                ImagemUrl = "/catalog/cartel-oxford.svg",
                Categoria = "Shirts",
                Descricao = "Structured Japanese cotton in a deep charcoal wash. The Cartel Oxford bridges ceremony and chaos — a button-down collar with attitude, barrel cuffs meant to be rolled to the forearm.",
                Destaque = false,
                Tamanhos = new List<string> { "S", "M", "L", "XL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 8,
                Nome = "Midnight Henley",
                Slug = "midnight-henley",
                Preco = 109m,
                ImagemUrl = "/catalog/midnight-henley.svg",
                Categoria = "Shirts",
                Descricao = "Heavyweight slub cotton with a three-button placket. The Midnight Henley sits between vulnerability and armor — a garment for the hours after the deal is done, when walls come down.",
                Destaque = false,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 9,
                Nome = "Eclipse Trousers",
                Slug = "eclipse-trousers",
                Preco = 179m,
                ImagemUrl = "/catalog/eclipse-trousers.svg",
                Categoria = "Trousers",
                Descricao = "Wide-leg trousers in a fluid wool-viscose blend. The Eclipse falls straight from the hip to a clean break at the shoe — pleated front, side-seam pockets, a silhouette stolen from Italian cinema.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L", "XL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 10,
                Nome = "Sovereign Cargos",
                Slug = "sovereign-cargos",
                Preco = 199m,
                ImagemUrl = "/catalog/sovereign-cargos.svg",
                Categoria = "Trousers",
                Descricao = "Washed cotton twill with articulated knees and concealed cargo pockets. The Sovereign rejects utility cliches — this is workwear elevated to ceremony, every pocket placed with a surgeon's precision.",
                Destaque = false,
                Tamanhos = new List<string> { "S", "M", "L", "XL", "XXL" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 11,
                Nome = "Gold Signet Ring",
                Slug = "gold-signet-ring",
                Preco = 129m,
                ImagemUrl = "/catalog/gold-signet-ring.svg",
                Categoria = "Accessories",
                Descricao = "18k gold-plated brass with a matte-finished face. The Signet Ring carries the Cartel crest in negative space — a mark of belonging, worn on the hand that seals the agreement.",
                Destaque = true,
                Tamanhos = new List<string> { "S", "M", "L" },
                ImagensAdicionais = new List<string>()
            },
            new()
            {
                Id = 12,
                Nome = "Onyx Chain",
                Slug = "onyx-chain",
                Preco = 159m,
                ImagemUrl = "/catalog/onyx-chain.svg",
                Categoria = "Accessories",
                Descricao = "Black rhodium-plated sterling silver with faceted onyx beads. The Onyx Chain rests against the chest like a quiet oath — 22-inch drop, lobster clasp, weight that reminds you it is there.",
                Destaque = true,
                Tamanhos = new List<string>(),
                ImagensAdicionais = new List<string>()
            }
        };
    }

    public IActionResult Index(string? categoria, string? pesquisa, string? ordem)
    {
        var produtos = GetProdutos();

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            produtos = produtos
                .Where(p => p.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(pesquisa))
        {
            var search = pesquisa.ToLower();
            produtos = produtos
                .Where(p => p.Nome.ToLower().Contains(search) ||
                            p.Descricao.ToLower().Contains(search) ||
                            p.Categoria.ToLower().Contains(search))
                .ToList();
        }

        produtos = ordem switch
        {
            "preco-asc"  => produtos.OrderBy(p => p.Preco).ToList(),
            "preco-desc" => produtos.OrderByDescending(p => p.Preco).ToList(),
            _            => produtos
        };

        ViewBag.CategoriaAtual = categoria;
        ViewBag.PesquisaAtual = pesquisa;
        ViewBag.OrdemAtual = ordem;

        return View(produtos);
    }

    public IActionResult Detalhes(int id)
    {
        var produtos = GetProdutos();
        var produto = produtos.FirstOrDefault(p => p.Id == id);

        if (produto == null)
        {
            return NotFound();
        }

        var relacionados = produtos
            .Where(p => p.Categoria == produto.Categoria && p.Id != produto.Id)
            .Take(4)
            .ToList();

        ViewBag.Relacionados = relacionados;

        return View(produto);
    }
}
