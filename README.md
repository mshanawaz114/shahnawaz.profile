# shahnawaz.profile

Personal portfolio for **Shahnawaz Mohammed** — Senior .NET Engineer, Solutions Architect, and Azure Specialist.

Built as a two-tier application:

- **`client/`** — Angular 21 single-page application (standalone components, signals, new control flow). Loads structured résumé / project data from bundled JSON, so the SPA renders independently of the backend.
- **`api/`** — Azure Functions (.NET 8 isolated worker) exposing the résumé-assistant chat endpoint, which proxies to the Groq LLM platform. Runs on Azure Static Web Apps' managed Functions API.

> Why Functions and not full ASP.NET Core Web API? Azure Static Web Apps' managed API runtime only accepts Azure Functions. The narrative target runtime in the portfolio (".NET 10") refers to forward-looking architect work; the chat function host is pinned to .NET 8 LTS, which is what SWA's managed Functions runtime currently supports.

Deployed to **Azure Static Web Apps** (free tier) with the client served from CDN and the Functions API attached as a linked managed API (`/api/*` is same-origin, no CORS).

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (LTS) — for the Functions API
- [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local) — for `func start`
- [Node.js 20+](https://nodejs.org/) and npm — for the Angular client
- A [Groq API key](https://console.groq.com/keys) — for the résumé assistant

---

## Local development

### 1. Configure secrets

```bash
cd api
cp local.settings.json.example local.settings.json
# Edit local.settings.json and set GROQ_API_KEY to your Groq key.
# This file is gitignored.
```

### 2. Start the Functions API

```bash
cd api
func start
```

The Functions host serves the API at `http://localhost:7071`. Endpoints:

- `POST /api/chat`    — résumé assistant (Groq-backed)
- `GET  /api/health`  — liveness probe

### 3. Start the Angular client

```bash
cd client
npm install
npm start   # ng serve on http://localhost:4200
```

The dev build reads `src/environments/environment.ts`, which points the chat call at the local Functions host. The page itself loads `/data/resume.json` and `/data/projects.json` — bundled into the Angular build via the `angular.json` asset glob from `../api/Data/` — so the portfolio renders even with the API offline.

---

## Production build

```bash
# Functions API
cd api && dotnet publish -c Release -o ./publish

# Angular client
cd ../client && npm ci && npm run build -- --configuration production
```

The production client reads `src/environments/environment.prod.ts`, which leaves `apiBase = ''` — HTTP calls go to `/api/*` against the same origin, which is exactly what Azure Static Web Apps with a linked Functions API exposes.

---

## Deployment

All deployment is via GitHub Actions to **Azure Static Web Apps**.

See [`DEPLOY-AZURE.md`](./DEPLOY-AZURE.md) for the full walkthrough (portal, secrets, custom domain).

Short version:

1. Create an Azure Static Web App resource (Free tier) connected to this repo.
2. Build config: app → `client`, output → `dist/client/browser`, api → `api/publish` with `skip_api_build: true` (the workflow runs `dotnet publish` first).
3. Add `GROQ_API_KEY` as an environment variable on the Static Web App.
4. Push to `main` — the workflow in `.github/workflows/azure-static-web-apps-*.yml` builds both halves and deploys.

---

## Repository layout

```
shahnawaz.profile/
├── api/                              Azure Functions (.NET 8 isolated worker)
│   ├── Functions/ChatFunction.cs    HTTP trigger — POST /api/chat, GET /api/health
│   ├── Services/                     GroqChatService, PromptBuilder, ContentProvider
│   ├── Models/                       ChatRequest, ChatMessage, GroqModels
│   ├── Data/                         resume.json, projects.json (source of truth)
│   ├── host.json                     Functions runtime config
│   ├── local.settings.json.example   Template for local secrets (copy → local.settings.json)
│   ├── appsettings.json              Groq:Model / Temperature / MaxTokens defaults
│   ├── Program.cs                    Functions host, DI, configuration
│   └── api.csproj
├── client/                           Angular 21 SPA
│   ├── src/app/
│   │   ├── components/               nav, hero, about, skills, experience, projects, contact, chat-widget, footer
│   │   ├── services/                 resume.service, chat.service, chat-toggle.service
│   │   ├── models/                   typed résumé / project / chat interfaces
│   │   └── app.ts / app.config.ts
│   ├── src/environments/             environment.ts, environment.prod.ts
│   ├── src/styles.scss               design system
│   ├── angular.json                  bundles api/Data/*.json into dist/client/browser/data/
│   └── package.json
├── .github/workflows/
│   └── azure-static-web-apps-*.yml   builds .NET 8 Functions + Angular, deploys to SWA
├── staticwebapp.config.json          SWA routing + headers
├── DEPLOY-AZURE.md                   Azure deployment walkthrough
└── README.md                         this file
```

---

## Tech notes

- **Standalone Angular components** with signals and the new `@if / @for` control flow.
- **Zoneless change detection** (`provideZonelessChangeDetection()`).
- **HttpClient with `withFetch()`** — works well with Azure SWA's edge.
- **Display decoupled from API**: `/data/*.json` is bundled with the client at build time, so the portfolio always renders even if the Functions API is offline. Only chat requires the backend.
- **Typed HttpClient** in the Functions worker via `IHttpClientFactory`, calling Groq's OpenAI-compatible REST endpoint.
- **WCAG 2.1**: skip-link, ARIA labels, respects `prefers-reduced-motion`.

## License

MIT — see [LICENSE](./LICENSE).
