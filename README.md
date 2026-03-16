# Olimpo Backend (`APIOlimpo`)

API em `.NET 8` para e-commerce com:
- catálogo de produtos e categorias
- criação de pedidos
- integração com Mercado Pago
- webhook para atualização de status de pagamento
- reserva/liberação automática de estoque
- envio de e-mail de confirmação

## Estrutura

- `Olimpo.ProductAPI` - API principal
- `Olimpo.Tests` - testes automatizados (`xUnit`)

## Stack

- `.NET 8` / `ASP.NET Core Web API`
- `Entity Framework Core` + `Pomelo` (MySQL)
- `AutoMapper`
- `Swagger`
- `Mercado Pago API`

## Pré-requisitos

- `.NET SDK 8`
- MySQL

## Configuração local

1. Copie os exemplos de configuração:
   - `Olimpo.ProductAPI/appsettings.Example.json` -> `Olimpo.ProductAPI/appsettings.json`
   - `Olimpo.ProductAPI/appsettings.Development.Example.json` -> `Olimpo.ProductAPI/appsettings.Development.json`
2. Preencha credenciais locais (DB, Mercado Pago e SMTP).
3. Aplique migrations no banco.
4. Execute a API.

## Ambiente Mercado Pago

- Produção:
  - `MercadoPago:UseSandbox = false`
  - token de produção (`APP_USR-...`)
- Desenvolvimento/Teste:
  - `MercadoPago:UseSandbox = true`
  - token de teste (`TEST-...`)

## Segurança de configuração

Arquivos com segredos **não devem ser versionados**:
- `Olimpo.ProductAPI/appsettings.json`
- `Olimpo.ProductAPI/appsettings.Development.json`

Use variáveis de ambiente, `User Secrets` ou secrets do provedor de hospedagem.

## Comandos úteis

- Build:
  - `dotnet build APIOlimpo.sln`
- Testes:
  - `dotnet test APIOlimpo.sln`
- Rodar API:
  - `dotnet run --project Olimpo.ProductAPI/Olimpo.ProductAPI.csproj`

## Fluxo de checkout

1. Front envia `POST /api/orders`.
2. API cria pedido, reserva estoque e gera preferência no Mercado Pago.
3. API retorna `paymentUrl`.
4. Front redireciona usuário para `paymentUrl`.
5. Mercado Pago chama `POST /api/webhooks/mercadopago`.
6. API atualiza status do pedido e estoque.

## Status do projeto

Projeto em fase final de integração com frontend e validação de deploy.
