# HYPE CARTEL - Contexto completo do projeto

Data de atualização: 19/03/2026

Este documento descreve o estado atual do projeto, a arquitetura, os fluxos principais e os pontos técnicos relevantes para manutenção e evolução.

## 1. Visão geral

HYPE CARTEL é uma aplicação web ASP.NET Core MVC orientada a e-commerce, com foco em catálogo de produtos, carrinho, autenticação por cookie e área administrativa para gestão de produtos.

Stack atual:
- .NET 10 (ASP.NET Core MVC)
- Razor Views
- CSS e JavaScript vanilla
- Persistência em ficheiros JSON

Projeto principal:
- MafiaStore.csproj

Solução:
- Hype-Cartel.sln

## 2. Objetivos funcionais implementados

Atualmente o sistema entrega:
- Navegação completa entre Home, Store, detalhe de produto, carrinho, login, registo e admin.
- Pesquisa e filtro de produtos.
- Adição, atualização de quantidade e remoção de itens no carrinho.
- Autenticação por cookie com roles.
- Gestão de produtos (CRUD) na área admin.
- Persistência de produtos e utilizadores em JSON com carregamento automático.

## 3. Arquitetura da aplicação

### 3.1 Estrutura em camadas

- Controllers:
  - Recebem requests HTTP e coordenam fluxo MVC.
- Services:
  - Encapsulam regras de negócio e persistência em ficheiros.
- Models/ViewModels:
  - Tipos para dados de domínio e transporte para as views.
- Views Razor:
  - Camada de apresentação.
- Filter global:
  - Injeta contagem de carrinho em todas as páginas.

### 3.2 Dependências e injeção de dependência

No arranque da aplicação, em Program.cs, são registados:
- IProductCatalogService -> ProductCatalogService
- ICartStore -> CartStore
- IUserStore -> UserStore

Também são configurados:
- Autenticação por cookie (HypeCartel.Auth)
- Autorização por role
- Pipeline MVC com filtro CartCountActionFilter

## 4. Rotas e controllers

### 4.1 HomeController

Responsável por páginas institucionais:
- GET /Home/Index
- GET /Home/Privacy
- GET /Home/Error

### 4.2 ProdutosController

Responsável por catálogo público:
- GET /Produtos/Index
  - Filtros suportados: categoria, pesquisa, ordem
  - Ordenação: preco-asc e preco-desc
- GET /Produtos/Detalhes/{id}
  - Carrega produto e produtos relacionados por categoria

### 4.3 CarrinhoController

Responsável por operações de carrinho:
- GET /Carrinho/Index
- POST /Carrinho/Adicionar
- POST /Carrinho/AtualizarQuantidade
- POST /Carrinho/Remover

Todos os POSTs usam ValidateAntiForgeryToken.

### 4.4 AccountController

Responsável por autenticação:
- GET /Account/Login
- POST /Account/Login
- GET /Account/Register
- POST /Account/Register
- POST /Account/Logout

No login, as claims Name e Role são emitidas no cookie.

### 4.5 AdminController

Protegido por Authorize(Roles = "Admin"):
- GET /Admin/Produtos
- POST /Admin/Criar
- POST /Admin/Editar
- POST /Admin/Remover

## 5. Serviços e regras de negócio

### 5.1 ProductCatalogService

Fonte principal de catálogo:
- Ficheiro de dados: Catalog_Assets/products.json
- Funcionalidades:
  - GetAll, GetById, GetNextId
  - Create, Update, Delete
  - Seed automático se o ficheiro não existir ou estiver vazio
- Implementa:
  - Escrita atómica com ficheiro temporário .tmp
  - Normalização de paths de imagem
  - Geração/normalização de slug
  - Validação de dados mínimos de produto
  - Mapeamento entre categoria de UI e categoryId do JSON

### 5.2 CartStore

Estado do carrinho em memória (singleton):
- Inicializa com itens de exemplo.
- Usa IProductCatalogService para obter dados válidos de produto ao adicionar.
- Chave de item composta por produto + tamanho.
- Regras:
  - Se item igual já existe, incrementa quantidade.
  - Quantidade <= 0 remove o item.

Observação:
- O carrinho atual é global na instância da aplicação, não por utilizador.

### 5.3 UserStore

Gestão de contas em JSON:
- Ficheiro de dados: context/users.json
- Funcionalidades:
  - Authenticate
  - CreateUser
  - FindByUsername
  - GetAll
- Segurança de password:
  - Salt aleatório por utilizador
  - Hash SHA-256 sobre salt + password
- Inclui seed/upsert automático de contas padrão.

Contas padrão:
- admin / Admin@123 (Admin)
- cliente / Cliente@123 (Customer)

## 6. Modelo de dados principal

### 6.1 ProdutoViewModel

Campos principais:
- Id, Nome, Slug
- Preco, Categoria
- ImagemUrl, ImagensAdicionais
- Descricao, Destaque
- Tamanhos

### 6.2 CarrinhoItemViewModel

Campos:
- Id, Nome, ImagemUrl
- Preco, Quantidade, Tamanho
- Subtotal calculado

### 6.3 UserAccount

Campos:
- Username, Email
- PasswordHash, PasswordSalt
- Role

### 6.4 ViewModels de autenticação

LoginViewModel:
- Username, Password, ReturnUrl

RegisterViewModel:
- Username, Email, Password, ConfirmPassword
- Validações por DataAnnotations

## 7. Frontend e experiência do utilizador

### 7.1 Layout global

No layout compartilhado:
- Navbar com links dinâmicos por role
- Badge de carrinho com contador global
- Overlay de pesquisa fullscreen
- Menu mobile com controlo de scroll do body
- Footer expandido com secção Customer Care

### 7.2 Catálogo e detalhe

No fluxo de produtos:
- Página de listagem com filtro, ordenação e pesquisa
- Breadcrumb visual
- Ação de adicionar ao carrinho via POST real
- Página de detalhe com seleção de tamanho
- Lista de produtos relacionados

### 7.3 Carrinho

Página de carrinho com:
- Incremento/decremento/remover via chamadas fetch para endpoints POST
- Token antiforgery enviado no payload
- Recarregamento da página após operação para manter consistência
- Estado visual para código promocional inválido

## 8. Segurança implementada

Medidas ativas:
- Autenticação por cookie com tempo de expiração e sliding expiration
- Autorização por role na área admin
- ValidateAntiForgeryToken nos POSTs sensíveis
- Forms com antiforgery token
- Verificação de ReturnUrl local no login

## 9. Infraestrutura e ficheiros estáticos

Configuração de estáticos:
- wwwroot para assets tradicionais
- Pasta Assets exposta via UseStaticFiles em /Assets
- MapStaticAssets ativo para otimização de estáticos

Configuração principal:
- appsettings.json
- appsettings.Development.json

## 10. Estado atual e limitações conhecidas

Estado atual:
- Projeto funcional para demonstração end-to-end de loja MVC.

Limitações conhecidas:
- Carrinho mantido em memória do servidor (não persistente por utilizador).
- JSON é adequado para protótipo, mas limitado para concorrência e auditoria.
- Hash de password com SHA-256 + salt funciona, mas algoritmo dedicado de password hashing seria mais robusto em produção.

## 11. Build e execução

Comando de build validado:
- dotnet build .\MafiaStore.csproj --nologo -o %TEMP%\hype-cartel-build-out -p:UseAppHost=false

Execução local típica:
- dotnet run --project .\MafiaStore.csproj

## 12. Próxima evolução recomendada

Evolução técnica prioritária:
- Migrar persistência JSON para SQLite mantendo contratos de interface.

Plano sugerido:
- Manter interfaces IProductCatalogService, IUserStore e ICartStore.
- Introduzir implementações SQLite com migrações.
- Tornar carrinho associado ao utilizador autenticado.
- Preservar controllers e views para minimizar impacto de UI.

Resultado esperado:
- Mais consistência de dados
- Melhor suporte a concorrência
- Base sólida para checkout, pedidos e histórico
