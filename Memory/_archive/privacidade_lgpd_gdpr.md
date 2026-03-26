# Privacidade LGPD/GDPR - Diretrizes

## Objetivo
Definir base de conformidade para tratamento de dados pessoais no ecommerce.

## Dados pessoais atualmente envolvidos
- Conta: username, email, credenciais (Identity hash).
- Pedido: user id, itens comprados, valores e timestamps.
- Sessao/carrinho: cookie identificador anonimo (`MafiaStore.CartOwner`).

## Gaps de privacidade no estado atual
- Sem politica de consentimento formal no front.
- Sem processo documentado de atendimento a direitos do titular.
- Sem matriz de retencao de dados.
- Sem registro de base legal por tratamento.

## Requisitos minimos a formalizar

### Transparencia
- Publicar politica de privacidade clara com:
  - quais dados sao coletados,
  - para quais finalidades,
  - com quem sao compartilhados,
  - prazos de retencao.

### Direitos do titular
- Canal para:
  - acesso,
  - correcao,
  - exclusao,
  - portabilidade (quando aplicavel),
  - oposicao e revogacao de consentimento.

### Minimizacao e retencao
- Coletar apenas dados necessarios ao fluxo de compra/operacao.
- Definir tempos de retencao por entidade (conta, pedido, logs).

### Seguranca
- Controle de acesso por necessidade.
- Registro de incidente com dados pessoais no runbook.

## Plano de implementacao documental
1. Publicar aviso de privacidade e termos.
2. Criar procedimento interno de atendimento de solicitacoes LGPD/GDPR.
3. Criar checklist de impacto de privacidade para novas features.

## Relacoes
- [[seguranca_owasp_checklist]]
- [[gestao_segredos_e_chaves]]
- [[runbook_incidentes]]
