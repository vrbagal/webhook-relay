# WebhookRelay

A self-hosted, open-source webhook management platform — a lightweight alternative to [Hookdeck](https://hookdeck.com) and [Svix](https://svix.com).

WebhookRelay receives webhooks from any provider, verifies HMAC signatures, stores every event, fans out deliveries to multiple target URLs, retries failures with exponential backoff, and provides a React dashboard for inspecting payloads, managing endpoints, and replaying events.

---

## Features

- **Multi-provider ingestion** — Stripe, GitHub, Twilio, and Generic (HMAC-SHA256) out of the box
- **Signature verification** — each provider's exact algorithm with constant-time comparison
- **Audit log** — every inbound request is stored, including rejected (invalid signature) events
- **Fan-out delivery** — forward one webhook to multiple target URLs simultaneously
- **Routing rules** — filter delivery per target using JSON path conditions (equals, contains, starts_with, exists, …)
- **Automatic retries** — exponential backoff with dead-letter after configurable max attempts
- **Duplicate detection** — provider event IDs deduplicated per endpoint
- **Replay** — re-deliver any stored event on demand
- **Real-time dashboard** — live updates via SignalR; inspect payloads with syntax highlighting
- **Multi-database** — SQLite (default), SQL Server, PostgreSQL

---

## Tech Stack

| Layer | Technology |
|---|---|
| API | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Real-time | ASP.NET Core SignalR |
| Queue | System.Threading.Channels (in-process) |
| Logging | Serilog |
| Frontend | React 19 + Vite 8 + TypeScript |
| UI | shadcn/ui + Tailwind CSS |
| Server state | TanStack Query v5 |
| Routing | React Router v7 |
| Forms | React Hook Form + Zod |

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 22+](https://nodejs.org/)

### 1. Clone the repo

```bash
git clone https://github.com/vrbagal/webhook-relay.git
cd webhook-relay
```

### 2. Run the API

The default database is SQLite — no setup required.

```bash
cd src/WebhookRelay.Api
dotnet run
# API available at https://localhost:5001 (or http://localhost:5000)
```

### 3. Run the dashboard

```bash
cd dashboard
npm install
npm run dev
# Dashboard available at http://localhost:3000
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

---

## Configuration

All settings live in `src/WebhookRelay.Api/appsettings.json` (or environment variables).

### Database providers

```json
// SQLite (default — zero config)
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=webhookrelay.db"
  }
}
```

```json
// SQL Server
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=WebhookRelay;Trusted_Connection=True;"
  }
}
```

```json
// PostgreSQL
{
  "DatabaseProvider": "PostgreSQL",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=webhookrelay;Username=postgres;Password=yourpassword"
  }
}
```

> **Note:** For SQL Server and PostgreSQL, run `dotnet ef database update` after first launch to apply migrations.

---

## Docker

```bash
cd deploy
docker compose up --build
```

Services started:
- `api` — ASP.NET Core API on port 5000
- `dashboard` — React app served via nginx on port 80
- `db` — SQL Server 2022 (when using the SqlServer profile)

---

## Sending Webhooks

### Ingest URL format

```
POST https://<host>/webhooks/<endpoint-id>
```

Each endpoint has its ingest URL shown in the dashboard and returned by the API.

### Generic provider (HMAC-SHA256)

```bash
# Compute signature
SECRET="your-signing-secret"
PAYLOAD='{"event":"payment.completed","amount":100}'
SIG=$(echo -n "$PAYLOAD" | openssl dgst -sha256 -hmac "$SECRET" | awk '{print $2}')

# Send
curl -X POST https://localhost:5001/webhooks/<endpoint-id> \
  -H "Content-Type: application/json" \
  -H "x-webhook-signature: sha256=$SIG" \
  -H "x-webhook-event: payment.completed" \
  -H "x-webhook-id: evt-$(date +%s)" \
  -d "$PAYLOAD"
```

### Stripe

Send requests as-is from Stripe — the `Stripe-Signature` header is verified automatically.

### GitHub

Send requests as-is from GitHub — the `X-Hub-Signature-256` header is verified automatically.

---

## API Reference

### Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/endpoints` | List all endpoints |
| `POST` | `/api/endpoints` | Create endpoint |
| `GET` | `/api/endpoints/:id` | Get endpoint |
| `PUT` | `/api/endpoints/:id` | Update endpoint |
| `DELETE` | `/api/endpoints/:id` | Delete endpoint |
| `POST` | `/api/endpoints/:id/targets` | Add delivery target |
| `DELETE` | `/api/endpoints/:id/targets/:targetId` | Remove delivery target |
| `GET` | `/api/endpoints/:id/targets/:targetId/rules` | List routing rules |
| `POST` | `/api/endpoints/:id/targets/:targetId/rules` | Add routing rule |
| `DELETE` | `/api/endpoints/:id/targets/:targetId/rules/:ruleId` | Delete routing rule |

### Events

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/events` | List events (paginated, filterable) |
| `GET` | `/api/events/:id` | Get event with delivery attempts |
| `POST` | `/api/events/:id/replay` | Replay event |

### Deliveries

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/api/deliveries` | List delivery attempts |

### Ingest

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/webhooks/:endpointId` | Receive a webhook |

---

## Routing Rules

Each delivery target can have one or more routing rules. All rules are AND-ed — a target is skipped unless every rule passes.

| Operator | Description |
|----------|-------------|
| `equals` | Field value equals (case-insensitive) |
| `not_equals` | Field value does not equal |
| `contains` | Field value contains substring |
| `not_contains` | Field value does not contain substring |
| `starts_with` | Field value starts with |
| `ends_with` | Field value ends with |
| `exists` | Field is present in the payload |
| `not_exists` | Field is absent from the payload |

**JSON path syntax:** dot-notation, e.g. `$.type`, `data.object.amount`, `action`

---

## Project Structure

```
WebhookRelay.sln
├── src/
│   ├── WebhookRelay.Api/          # ASP.NET Core Web API + background workers
│   ├── WebhookRelay.Core/         # Domain entities, interfaces (no external deps)
│   ├── WebhookRelay.Infrastructure/  # EF Core, HTTP delivery, verifiers
│   └── WebhookRelay.Shared/       # DTOs shared between API and frontend types
├── dashboard/                     # React 19 + Vite 8 frontend
├── tests/
│   ├── WebhookRelay.Core.Tests/   # Unit tests (xUnit)
│   ├── WebhookRelay.Api.Tests/    # API tests (WebApplicationFactory)
│   └── WebhookRelay.Integration.Tests/
├── tools/
│   └── WebhookRelay.TestSender/   # CLI tool for sending test webhooks
└── deploy/                        # Docker Compose + Dockerfiles + nginx
```

---

## Running Tests

```bash
dotnet test
```

---

## License

MIT
