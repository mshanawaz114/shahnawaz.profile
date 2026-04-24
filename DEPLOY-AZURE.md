# Deploying to Azure Static Web Apps (Free Tier)

This app runs on **Azure Static Web Apps** with the client (Angular 21) served from CDN and the chat backend (.NET 8 Azure Functions) attached as a **linked managed API**. Both halves ship together from a single GitHub Actions workflow and share one origin, so the browser never makes cross-origin calls.

**Monthly cost on the Free tier: $0**, with 100 GB bandwidth, 2 custom domains, and free managed TLS.

> **Why Azure Functions, not ASP.NET Core Web API?** SWA's managed API runtime only accepts Azure Functions. A regular Web API project would deploy but every `/api/*` request would return HTTP 405 because the Functions runtime can't find a matching function. The chat API is structured as an isolated-worker .NET 8 Functions project for that reason.

---

## 1. Prerequisites

- An Azure account (free tier works — [free signup](https://azure.microsoft.com/en-us/free/)).
- This repo pushed to `github.com/mshanawaz114/shahnawaz.profile` on `main`.
- A [Groq API key](https://console.groq.com/keys).

---

## 2. Create the Static Web App

1. Go to [portal.azure.com](https://portal.azure.com) → **Create a resource** → search for **Static Web App** → **Create**.
2. **Basics:**
   - Subscription / Resource group — pick or create.
   - Name — `shahnawaz-profile` (or similar).
   - Plan type — **Free**.
   - Region — pick the closest to you.
   - Source — **GitHub**. Sign in and authorize.
3. **Deployment details:**
   - Organization — `mshanawaz114`
   - Repository — `shahnawaz.profile`
   - Branch — `main`
4. **Build details:**
   - Build preset — **Angular** (auto-detected) or **Custom** — both fine.
   - App location — `client`
   - API location — `api/publish`
   - Output location — `dist/client/browser`
5. Click **Review + create** → **Create**.

Azure will:
- Provision the Static Web App resource.
- Commit a generated GitHub Actions workflow to your repo at `.github/workflows/azure-static-web-apps-<random>.yml`.
- Add an `AZURE_STATIC_WEB_APPS_API_TOKEN_<RANDOM>` secret to your repo.

The committed workflow file in this repo has been **modified** from Azure's default to add a `dotnet publish` step before the SWA deploy. If you re-create the SWA resource (and Azure regenerates the workflow), copy the `dotnet publish` step back in — see the existing file for reference.

---

## 3. Add the Groq API key

1. Open your Static Web App in the portal.
2. Left nav → **Settings → Environment variables**.
3. Click **+ Add** and create:
   - Name: `GROQ_API_KEY`
   - Value: `gsk_...` (your Groq key)
4. Save.

The Functions worker picks this up via `IConfiguration["GROQ_API_KEY"]` (which reads env vars in Azure Functions hosts) — see `api/Services/GroqChatService.cs`.

You can optionally override defaults too:
- `Groq__Model` — defaults to `llama-3.3-70b-versatile` (note the **double underscore** — that's how .NET maps env vars to nested config keys)
- `Groq__Temperature` — defaults to `0.4`
- `Groq__MaxTokens` — defaults to `800`

---

## 4. First deployment

Push any change to `main`, or re-run the workflow from the **Actions** tab on GitHub. The run will:

1. Install .NET 8 SDK.
2. Publish the .NET 8 Functions API (`dotnet publish -c Release -o ./publish`).
3. Hand the published artifacts + Angular source to SWA's Oryx builder.
4. Oryx runs `npm ci && npm run build` for the Angular client.
5. SWA uploads the static client + Functions bundle to the global edge.

When green, Azure will show the URL under **Overview → URL**, e.g. `https://kind-sand-054c25910.7.azurestaticapps.net`.

---

## 5. Custom domain (optional)

1. **Settings → Custom domains → + Add**.
2. Choose **Custom domain on other DNS** (or an Azure-managed domain).
3. Follow the prompts to add the `CNAME` (or `TXT` + `ALIAS`) record at your registrar.
4. Azure auto-provisions a TLS certificate once DNS propagates.

---

## 6. Verifying

Once deployed:

- `https://<your-swa>/` → Angular portfolio.
- `https://<your-swa>/api/health` → `{ "status": "ok", "runtime": ".NET 8 isolated (Azure Functions on Static Web Apps managed API)" }`.
- Click the chat launcher → send a message → should get a Groq-powered reply.

If the chat returns "chat service is not configured", the `GROQ_API_KEY` env var is missing or incorrectly named. Re-check step 3.

If the chat returns HTTP 405, the Functions API didn't deploy — check the workflow log for a successful `dotnet publish` step and confirm `api_location: api/publish` + `skip_api_build: true`.

---

## 7. Day-2 operations

- **Logs** — Static Web Apps → *Monitoring → Log stream* for Functions logs.
- **Rotate Groq key** — update the `GROQ_API_KEY` env var; no redeploy needed.
- **Environment slots** — PR previews are automatic; the workflow closes them on merge.
- **Bumping the Functions runtime** — when SWA managed API supports a newer runtime, update `api/api.csproj` `<TargetFramework>` and the workflow's `actions/setup-dotnet` `dotnet-version`.

---

## 8. Troubleshooting

| Symptom | Fix |
|---|---|
| Chat returns HTTP 405 | The Functions API didn't deploy. Confirm `dotnet publish` ran and `api_location: api/publish` + `skip_api_build: true` are set in the workflow. |
| Chat returns HTTP 500 with "chat service is not configured" | `GROQ_API_KEY` env var missing or misspelled in SWA → Settings → Environment variables. |
| Chat returns HTTP 502 | Groq upstream rejected the request — check the API logs in **Monitoring → Log stream** for the Groq response. Most likely: deprecated model name (override `Groq__Model`) or invalid API key. |
| Workflow fails at `dotnet publish` | Local sanity check: `cd api && dotnet publish -c Release` should succeed against .NET 8 SDK. |
| Workflow fails at `npm ci` | Commit `client/package-lock.json` (generated by first `npm install`) to the repo. |
| Angular routes 404 on refresh | `staticwebapp.config.json` has `navigationFallback` already — ensure it's at repo root and committed. |
| Two workflows running and racing | Delete any extras under `.github/workflows/`. Keep only the one whose secret name matches the `AZURE_STATIC_WEB_APPS_API_TOKEN_*` secret in your GitHub repo settings. |
