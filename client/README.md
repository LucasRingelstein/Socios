# Socios Subscription Client

React + Vite single-page client for creating Mercado Pago subscriptions via the backend `POST /subscriptions` endpoint and monitoring payment status.

## Environment

Copy `.env.example` to `.env` and set your backend base URL and Mercado Pago public key:

```
VITE_API_BASE_URL=http://localhost:3000
VITE_MERCADO_PAGO_PUBLIC_KEY=YOUR_PUBLIC_KEY
```

## Development

```
npm install
npm run dev
```

## How it works
- Loads the Mercado Pago JS SDK with the configured public key.
- Collects payer information and plan selection, then calls `POST /subscriptions` on the backend.
- Starts Checkout using the returned preference/preapproval identifier (or `init_point` fallback).
- Polls `GET /subscriptions/{id}` for status and attempts server-sent events from `/subscriptions/{id}/events` when available.
- Provides loading and error states throughout the checkout and status flows.
