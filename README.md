# shahnawaz.profile

Personal portfolio for **Shahnawaz Mohammed** — Senior .NET Engineer, Solutions Architect, and Azure Specialist.

Built as a two-tier application:

- **`client/`** — Angular 21 single-page application (standalone components, signals, new control flow).
- **`api/`** — ASP.NET Core 10 Web API serving résumé JSON and proxying the résumé-assistant chat to the Groq LLM platform.

Deployed to **Azure Static Web Apps** (free tier) with the client served from CDN and the .NET API attached as a linked managed API (`/api/*` is same-origin, no CORS).

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (preview channel)
- [Node.js 20+](https://nodejs.org/) and npm
- A [Groq API key](https://console.groq.com/keys) for the résumé assistant

---

## Local development

### 1. Start the .NET API

```bash
cd api
# one-time
dotnet restore

# set your Groq key (choose one)
export GROQ_API_KEY=gsk_...
# or persist via user-secrets:
# dotnet user-secrets init
# dotnet user-secrets set "GROQ_API_KEY" "gsk_..."

dotnet run
```

Kestrel serves the API at `http://localhost:5000` by default. Endpoints:

- `GET  /api/resume`   — résumé JSON
- `GET  /api/projects` — case-study JSON
- `POST /api/chat`     — résumé assistant (Groq-backed)
- `GET  /api/health`   — liveness probe

### 2. Start the Angular client

```bash
cd client
npm install
npm start   # ng serve on http://localhost:4200
```

The dev build reads `src/environments/environment.ts` which points at
`http://localhost:5000` for the API. Override this file if your Kestrel port differs.

---

## Production build

```bash
# API
cd api && dotnet publish -c Release -o ./publish

# Client
cd ../client && npm ci && npm run build -- --configuration production
```

The production client reads `src/environments/environment.prod.ts`, which leaves
`apiBase = ''` — HTTP calls go to `/api/*` against the same origin, which is exactly
what Azure Static Web Apps with a linked API exposes.

---

## Deployment

All deployment is via GitHub Actions to **Azure Static Web Apps**.

See [`DEPLOY-AZURE.md`](./DEPLOY-AZURE.md) for the full walkthrough (portal, secrets, custom domain).

Short version:

1. Create an Azure Static Web App resource (Free tier) connected to this repo.
2. Set build config: app → `client`, output → `dist/client/browser`, api → `api/publish`.
3. Add `GROQ_API_KEY` as an application setting on the Static Web App.
4. Push to `main` — the workflow in `.github/workflows/azure-static-web-apps.yml` builds both halves and deploys.

---

## Repository layout

```
shahnawaz.profile/
├── api/                         ASP.NET Core 10 Web API
│   ├── Controllers/             Chat, Resume, Projects endpoints
│   ├── Services/                GroqChatService, PromptBuilder, ContentProvider
│   ├── Models/                  ChatRequest, ChatMessage, GroqModels
│   ├── Data/                    resume.json, projects.json (source of truth)
│   ├── appsettings*.json
│   ├── Program.cs               minimal hosting, DI, CORS
│   └── api.csproj
├── client/                      Angular 21 SPA
│   ├── src/app/
│   │   ├── components/          nav, hero, about, skills, experience, projects, contact, chat-widget, footer
│   │   ├── services/            resume.service, chat.service, chat-toggle.service
│   │   ├── models/              typed résumé / project / chat interfaces
│   │   ├── app.ts / app.config.ts
│   ├── src/environments/        environment.ts, environment.prod.ts
│   ├── src/styles.scss          design system
│   ├── src/index.html
│   ├── angular.json
│   └── package.json
├── .github/workflows/
│   └── azure-static-web-apps.yml
├── staticwebapp.config.json     SWA routing + headers + API runtime
├── DEPLOY-AZURE.md              Azure deployment walkthrough
└── README.md                    this file
```

---

## Tech notes

- **Standalone Angular components** with signals and the new `@if / @for` control flow.
- **Zoneless change detection** (`provideZonelessChangeDetection()`).
- **HttpClient with `withFetch()`** — works well with Azure SWA's edge.
- **Typed HttpClient** on the .NET side via `IHttpClientFactory`, calling Groq's OpenAI-compatible REST endpoint.
- **WCAG 2.1**: skip-link, ARIA labels, respects `prefers-reduced-motion`.

## License

MIT — see [LICENSE](./LICENSE).
