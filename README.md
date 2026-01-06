# Socios Development Setup

This repository contains both the ASP.NET Core backend and the React frontend. Use the following guide to configure Mercado Pago credentials, set up HTTPS for local development, and run both apps together with CORS enabled.

## Backend configuration (`appsettings.Development.json`)

Populate `appsettings.Development.json` with your Mercado Pago credentials, the API base URL, and allowed frontend origins for CORS.

```json
{
  "MercadoPago": {
    "PublicKey": "YOUR_MERCADO_PAGO_PUBLIC_KEY",
    "AccessToken": "YOUR_MERCADO_PAGO_ACCESS_TOKEN",
    "WebhookSecret": "YOUR_MERCADO_PAGO_WEBHOOK_SECRET",
    "WebhookSigningKey": "YOUR_MERCADO_PAGO_WEBHOOK_SIGNING_KEY",
    "ApiBaseUrl": "https://api.mercadopago.com"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://localhost:5173"
    ]
  },
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  }
}
```

> Keep the actual values out of source control. If you prefer to store secrets outside the JSON file, use environment variables that map to these keys (e.g., `MercadoPago__AccessToken`).

## Frontend configuration (`client/.env`)

Create `client/.env` (or `.env.local`) for the React app with the same Mercado Pago settings. Vite-based projects expect `VITE_` prefixes.

```
VITE_MERCADO_PAGO_PUBLIC_KEY=YOUR_MERCADO_PAGO_PUBLIC_KEY
VITE_MERCADO_PAGO_ACCESS_TOKEN=YOUR_MERCADO_PAGO_ACCESS_TOKEN
VITE_MERCADO_PAGO_WEBHOOK_SECRET=YOUR_MERCADO_PAGO_WEBHOOK_SECRET
VITE_MERCADO_PAGO_WEBHOOK_SIGNING_KEY=YOUR_MERCADO_PAGO_WEBHOOK_SIGNING_KEY
VITE_MERCADO_PAGO_API_BASE_URL=https://api.mercadopago.com
```

## Local HTTPS certificate

Use the built-in developer certificate to enable HTTPS in ASP.NET Core:

```bash
# Create and trust the certificate on your machine
 dotnet dev-certs https --trust

# Verify the certificate status
 dotnet dev-certs https --check
```

If you prefer to use a custom certificate, place it under `certs/localhost.pfx` (do not commit it) and set the matching path/password via environment variables:

```bash
export ASPNETCORE_Kestrel__Certificates__Default__Path="$(pwd)/certs/localhost.pfx"
export ASPNETCORE_Kestrel__Certificates__Default__Password="changeit"
```

## CORS for the React dev server

Add a CORS policy in `Program.cs` (or `Startup.cs`) that allows the React dev server origin defined in `appsettings.Development.json`.

```csharp
var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
const string FrontendPolicy = "Frontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: FrontendPolicy, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();
app.UseHttpsRedirection();
app.UseCors(FrontendPolicy);
// other middleware
app.Run();
```

## Running backend and frontend together

Open two terminals from the repository root:

```bash
# Terminal 1: run the ASP.NET Core backend with hot reload
 dotnet watch run --project src/Socios.Api/Socios.Api.csproj

# Terminal 2: run the React app (assuming Vite and a `client` directory)
 cd client && npm install && npm run dev -- --host --port 5173
```

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
