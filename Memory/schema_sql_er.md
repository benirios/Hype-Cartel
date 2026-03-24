# Schema SQL / ER - Estado Atual

## Objetivo
Documentar entidades principais e relacoes persistidas no EF Core.

## Entidades de dominio
- `Category`
  - Id, Name, Slug
- `Product`
  - Id, Name, Slug, Sku, Price, Description, ImageUrl, AdditionalImagesJson, SizesJson, Stock, Highlight, CategoryId
- `Cart`
  - Id, OwnerKey, UpdatedAtUtc
- `CartItem`
  - Id, CartId, ProductId, Size, Quantity
- `Order`
  - Id, UserId, CreatedAtUtc, Status, Subtotal, Vat, Total
- `OrderLine`
  - Id, OrderId, ProductId, ProductName, Size, Quantity, UnitPrice
- `OrderHistory`
  - Id, OrderId, FromStatus, ToStatus, ChangedAtUtc, ChangedBy

## Entidades de identidade (Identity)
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- demais tabelas padrao do Identity

## Relacoes-chave
- Category 1:N Product
- Cart 1:N CartItem
- Order 1:N OrderLine
- Order 1:N OrderHistory
- User (Identity) 1:N Order (via UserId string)

## Constraints e indices relevantes (ApplicationDbContext)
- Unique index: `Category.Slug`
- Unique index: `Product.Slug`
- Unique index: `Product.Sku`
- Unique composite: `CartItem (CartId, ProductId, Size)`
- Index: `OrderHistory.OrderId`
- Precision: `Product.Price`, `Order.Subtotal`, `Order.Vat`, `Order.Total`, `OrderLine.UnitPrice` em 18,2

## Politicas de delete
- Product -> Category: Restrict
- OrderLine -> Order: Cascade
- OrderHistory -> Order: Cascade
- CartItem -> Cart: Cascade

## Migrations atuais
- `20260323131447_InitialCreate`
- `20260323131755_AddIdentityTables`
- `20260323202901_AddCartTables`
- `20260323205602_AddOrderStateManagement`

## Gaps de dados para operacao real
- endereco de entrega e frete ainda nao modelados,
- pagamentos/transacoes ainda nao modelados,
- auditoria de admin actions ainda nao modelada,
- consentimentos de privacidade nao modelados.

## Relacoes
- [[arquitetura_componentes]]
- [[checkout_pagamentos_estrategia]]
- [[logistica_envio_devolucao]]
