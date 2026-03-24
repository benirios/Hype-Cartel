# Matriz de Requisitos e Rastreabilidade

## Objetivo
Mapear requisitos de negocio para implementacao atual, gaps e backlog associado.

## Legenda
- Status:
  - Implementado
  - Parcial
  - Nao implementado

## Matriz

| ID | Requisito | Estado atual | Evidencia no repositorio | Gap / Proxima acao | Backlog |
|---|---|---|---|---|---|
| FR-01 | Catalogo com filtros/pesquisa/paginacao | Implementado | `Controllers/ProdutosController.cs`, `Views/Produtos/Index.cshtml` | Refinar SEO de URLs | BG-104 |
| FR-02 | Detalhe de produto com selecao de tamanho | Implementado | `Views/Produtos/Detalhes.cshtml` | Incluir provas sociais/reviews | BG-203 |
| FR-03 | Carrinho com atualizar/remover itens | Implementado | `Controllers/CarrinhoController.cs`, `Services/CartStore.cs` | Expor frete e prazo no carrinho | BG-002 |
| FR-04 | Checkout transacional com stock | Parcial | `Services/OrderService.cs` | Falta pagamento real e endereco | BG-001, BG-002 |
| FR-05 | Historico de encomendas por utilizador | Implementado | `Controllers/EncomendasController.cs`, `Views/Encomendas/*` | Notificacoes e tracking | BG-101, BG-102 |
| FR-06 | Gestao admin de produtos/categorias | Implementado | `Controllers/AdminController.cs`, `Views/Admin/Dashboard.cshtml` | Permissoes granulares admin | BG-105 |
| FR-07 | Gestao admin de pedidos e estados | Implementado | `AdminController.UpdateOrderStatus`, `Models/OrderStatus.cs` | Runbook de tratamento de excecoes | BG-005 |
| FR-08 | Gestao de utilizadores e roles | Implementado | `AdminController.UpdateUserRole`, `ToggleUserActive` | Auditoria de acesso admin | BG-105 |
| FR-09 | Relatorios de negocio no backoffice | Implementado | `AdminController.Dashboard` (tabs reports) | Exportacao CSV/PDF | BG-103 |
| FR-10 | Persistencia relacional com migracoes | Implementado | `Data/ApplicationDbContext.cs`, `Migrations/*` | Backup/restore formal | BG-005 |
| NFR-01 | Autenticacao e autorizacao robusta | Parcial | `Program.cs`, `AccountController.cs` | Rate-limit, 2FA, reset real | BG-003 |
| NFR-02 | Seguranca OWASP basica | Parcial | antiforgery + auth atual | Falta checklist e politicas completas | BG-003 |
| NFR-03 | Qualidade com testes automatizados | Nao implementado | `Tests/**` inexistente | Criar suite minima | BG-004 |
| NFR-04 | Observabilidade operacional | Nao implementado | logging default apenas | Logs estruturados, health, alertas | BG-005 |
| NFR-05 | SEO e indexacao | Parcial | metadados basicos no layout | sitemap/robots/schema/canonical | BG-104 |

## Uso pratico
- Cada PR relevante deve apontar para 1+ IDs desta matriz.
- Sempre que um requisito mudar de estado, atualizar:
  - `Estado atual`,
  - `Evidencia`,
  - `Backlog`.

## Relacoes
- [[backlog_priorizado]]
- [[roadmap_produto]]
- [[qa_checklist_funcional]]
