# Socios API

ASP.NET Core Web API for managing subscriptions and Mercado Pago webhook processing.

## Prerequisites

- .NET 8 SDK
- SQLite (default) or another EF Core provider of your choice

> If you cannot install .NET packages via `apt`, you can still use the [dotnet-install](https://learn.microsoft.com/dotnet/core/tools/dotnet-install-script) script locally to restore and build.

## Setup

1. Restore packages and build:

   ```bash
   dotnet restore
   dotnet build
   dotnet ef database update # optional if you add migrations
   dotnet run
   ```

2. Configure Mercado Pago credentials via environment variables or user secrets:

   - `MercadoPago__AccessToken`
   - `MercadoPago__PublicKey`
   - `MercadoPago__WebhookSecret` (shared secret for webhook signature validation)
   - `MercadoPago__WebhookCallbackUrl` (deployed webhook endpoint, e.g., `https://yourapp.com/webhooks/mercadopago`)
   - `MercadoPago__BackUrl` (return URL after subscription approval)

   To set user secrets in development:

   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "MercadoPago:AccessToken" "<access_token>"
   dotnet user-secrets set "MercadoPago:PublicKey" "<public_key>"
   dotnet user-secrets set "MercadoPago:WebhookSecret" "<webhook_secret>"
   ```

3. Update the webhook endpoint URL in the Mercado Pago dashboard to point to `/webhooks/mercadopago` on your deployed host.

## API

- `POST /subscriptions` — Create a subscription/preapproval. Provide plan details, payer email, and optional return/callback URLs.
- `GET /subscriptions/{id}` — Retrieve subscription status (fetches latest status from Mercado Pago when possible).
- `POST /webhooks/mercadopago` — Receive Mercado Pago webhooks. Validates signatures (if configured) and records status changes.

Swagger is enabled in development for easy testing.
