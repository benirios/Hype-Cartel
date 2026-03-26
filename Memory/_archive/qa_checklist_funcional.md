# QA Checklist Funcional

## Objetivo
Padronizar validacao funcional antes de cada release.

## Checklist Frontoffice
- [ ] Home carrega com produtos destaque.
- [ ] Catalogo aplica filtro por categoria corretamente.
- [ ] Pesquisa retorna itens relevantes.
- [ ] PDP abre sem erro e adiciona ao carrinho.
- [ ] Carrinho permite atualizar/remover itens.
- [ ] Checkout bloqueia carrinho vazio.
- [ ] Checkout bloqueia stock insuficiente.
- [ ] Mensagens de sucesso/erro aparecem corretamente.
- [ ] Usuario autenticado ve "My Orders".

## Checklist Area do cliente
- [ ] Login com credencial valida.
- [ ] Login falho mostra erro sem quebrar fluxo.
- [ ] Registro cria utilizador Customer.
- [ ] Historico lista apenas pedidos do utilizador logado.
- [ ] Detalhes da encomenda mostram linhas corretas.

## Checklist Admin Dashboard
- [ ] Aba overview renderiza KPIs sem excecao.
- [ ] Aba products cria/edita/remove produto.
- [ ] Alteracao de preco/stock persiste em DB.
- [ ] Aba categories cria/edita/remove categoria.
- [ ] Bloqueio de delete categoria com produtos funciona.
- [ ] Aba orders atualiza estado com transicao valida.
- [ ] Historico de estados e gravado.
- [ ] Aba users pesquisa, altera role e ativa/desativa.
- [ ] Regras de ultimo admin ativo sao respeitadas.
- [ ] Aba reports renderiza metricas sem erro.

## Checklist tecnico rapido
- [ ] `dotnet build ./MafiaStore.csproj` verde.
- [ ] `dotnet test ./MafiaStore.csproj` verde.
- [ ] `dotnet test ./Hype-Cartel.sln` verde (quando projeto de testes existir).

## Definicao de pronto para release
- Todos os itens P0 validados.
- Sem erro bloqueador em fluxo de compra.
- Sem regressao critica na dashboard admin.

## Relacoes
- [[matriz_testes_e2e]]
- [[plano_releases]]
- [[runbook_incidentes]]
