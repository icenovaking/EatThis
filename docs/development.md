# EatThis local development

Prerequisites:

- .NET SDK 10
- Node.js 24 or a current LTS release with npm
- A server-side Google Places key configured through .NET user secrets when live provider calls are enabled

Start the API from the repository root:

```powershell
dotnet run --project src/EatThis.Api/EatThis.Api.csproj --launch-profile EatThis.Api
```

The API listens on `http://localhost:5080`. Keep the Google key in user secrets or the deployment secret manager; the checked-in `appsettings.example.json` is intentionally empty.

Start the Vue frontend in a second terminal:

```powershell
Set-Location src/EatThis.Web
npm install
npm run dev
```

Vite serves the frontend on `http://localhost:5173` and proxies `/api` requests to the local API. If the frontend is deployed separately, set `VITE_API_BASE_URL` to the EatThis API origin in a non-secret environment file; never put a Google Places endpoint or credential there.

Useful checks:

```powershell
npm run test:run
npm run build
npm run security:check
```
