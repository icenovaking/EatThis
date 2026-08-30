# EatThis secret configuration

The Google Places credential belongs only to the ASP.NET Core API process. The Vue application must never receive it, include it in a bundle, or send it as a request field.

## Local development

Create the user-secrets store from `src/EatThis.Api` and set the key outside tracked files:

```powershell
dotnet user-secrets init --project src/EatThis.Api/EatThis.Api.csproj
dotnet user-secrets set "GooglePlaces:ApiKey" "<development-key>" --project src/EatThis.Api/EatThis.Api.csproj
```

Do not add `appsettings.Development.json`, `appsettings.Local.json`, `secrets*.json`, or frontend `.env` files containing credentials to Git. `appsettings.example.json` is the only committed example and intentionally contains an empty key.

## Deployment

Inject the production credential through the deployment secret manager or environment variable named `GooglePlaces__ApiKey`. Use a separate Google Cloud key from development, restrict it to the Places API used by the backend, and apply server-side application restrictions supported by the hosting environment.

The public frontend configuration may contain only the EatThis API base URL. It must not contain `GooglePlaces__ApiKey`, `X-Goog-Api-Key`, or a Google Places endpoint.
