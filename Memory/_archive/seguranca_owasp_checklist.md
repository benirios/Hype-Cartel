# Seguranca OWASP Checklist

## Objetivo
Manter um checklist pratico para reduzir risco de seguranca no ecommerce com base em OWASP Top 10.

## Referencia
- OWASP Top 10 (ultima versao publicada no portal OWASP: 2025).
- URL: https://owasp.org/www-project-top-ten/

## Snapshot atual do projeto
- Implementado:
  - `[Authorize]` e `[Authorize(Roles = "Admin")]` em areas sensiveis.
  - antiforgery em formularios POST.
  - EF Core (mitiga SQL injection classico por parametrizacao).
  - HTTPS/HSTS em producao.
- Gap:
  - sem rate limit de login,
  - sem politica de auditoria estruturada,
  - sem processo formal de gestao de segredos,
  - sem monitorizacao de eventos de seguranca.

## Checklist por area OWASP

### A01 Broken Access Control
- [x] Rotas admin protegidas por role.
- [x] Encomendas do cliente filtradas por `UserId`.
- [ ] RBAC granular no admin (operador vs super-admin).
- [ ] Auditoria de mudanca de role/estado de conta.

### A02 Cryptographic Failures
- [x] Cookies com HttpOnly e politica secure em producao.
- [x] HTTPS redirection.
- [ ] Segredos fora de `appsettings*.json`.
- [ ] Encriptacao de dados sensiveis em repouso (quando aplicavel).

### A03 Injection
- [x] Queries de dados via EF Core LINQ.
- [ ] Revisao de output encoding em pontos de input livre.
- [ ] Politica CSP para reduzir vetores de script injection.

### A04 Insecure Design
- [ ] Threat modeling dos fluxos criticos (checkout/admin).
- [ ] Requisitos de abuso/fraude no desenho de pagamento.

### A05 Security Misconfiguration
- [x] Diferenciacao basica de ambiente em `Program.cs`.
- [ ] Hardening de headers (CSP, X-Frame-Options, Referrer-Policy).
- [ ] Revisao de exposicao de erro detalhado em producao.

### A06 Vulnerable and Outdated Components
- [ ] Processo de update mensal de pacotes/NuGet.
- [ ] SCA automatizado no pipeline.

### A07 Identification and Authentication Failures
- [x] ASP.NET Identity ativo.
- [ ] Lockout em falha de login habilitado.
- [ ] Password reset com token e canal email real.
- [ ] 2FA para contas admin.

### A08 Software and Data Integrity Failures
- [ ] Assinatura/verificacao de artefatos de deploy.
- [ ] Protecao de branch e revisao obrigatoria em PR.

### A09 Security Logging and Monitoring Failures
- [ ] Logs estruturados de login/admin.
- [ ] Alertas para eventos criticos (falhas repetidas, mutacoes admin).
- [ ] Correlacao de request-id em incidentes.

### A10 SSRF
- [ ] Politica para chamadas externas (allowlist, timeout, retry controlado).
- [ ] Validacao de URLs de integracoes futuras.

## Prioridade de execucao (P0)
1. Rate limit login + lockout.
2. Gestao de segredos fora do repositorio.
3. Logging de eventos de seguranca.
4. Headers de seguranca e baseline CSP.

## Relacoes
- [[autenticacao_autorizacao_politicas]]
- [[gestao_segredos_e_chaves]]
- [[runbook_incidentes]]
