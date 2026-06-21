---
name: add-options
description: Add a strongly-typed configuration section to a RPGManager project and bind it via the options pattern, then wire its values into the local secret stores. Use when introducing a new appsettings section (e.g. "add a Stripe config section", "make the retry count configurable", "read the SMTP settings from config"). Creates the sealed options class with a SectionName const, binds it with services.Configure<T>, adds empty placeholders to appsettings.json, and sets the real values in .NET user-secrets and .env.
---

# Add a strongly-typed config section

RPGManager binds configuration through the **options pattern** (`IOptions<T>`), never by reading raw `IConfiguration` keys at call sites. Models on `Caching` (`CacheOptions`) and `Authentication` (`AuthenticationSettings`). Real secrets live in **.NET user-secrets** (local `dotnet run`) and **`.env`** (Docker Compose) — `appsettings.json` holds only empty placeholders, so nothing secret is committed.

## 1. Options class — `<Project>/.../Configuration/<Name>Options.cs`

- `public sealed class`, a `public const string SectionName`, defaults on every property.
- Place it in the project that owns the concern (a utility/integration library, or `WebApi.Presentation/Common/Options/` for app-level settings like auth).
- Nest a child `sealed class` for sub-objects (see `SwaggerAuthenticationSettings` inside `AuthenticationSettings`).

```csharp
namespace BlobStorage.Configuration;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public string ServiceUrl { get; set; } = string.Empty;
    public TimeSpan PresignedUrlExpiry { get; set; } = TimeSpan.FromMinutes(5);
}
```

## 2. Bind it in DI

In the owning project's DI extension, bind the section:

```csharp
services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));
```

Then **consume via injection** — `IOptions<BlobStorageOptions>` (read `.Value` once in a ctor) for long-lived services, or `IOptionsSnapshot<T>` if you need per-request reload. When a value is needed *during* registration (to configure a client), read it eagerly: `configuration.GetSection(<T>.SectionName).Get<T>()` (see `AddJwtAuthentication` / `AddCachingSupport`).

## 3. Placeholders — `appsettings.json`

Add the section with **empty / non-secret defaults**, matching the existing `Authentication` and `Cache` style. Never commit real credentials here.

```json
"BlobStorage": {
  "ServiceUrl": "",
  "AccessKey": "",
  "SecretKey": "",
  "PresignedUrlExpiry": "00:05:00"
}
```

Non-secret structural defaults (e.g. a region label, a boolean flag) may keep real values; secrets and environment-specific URLs stay empty.

## 4. Real values — user-secrets (local `dotnet run`)

The startup project `WebApi.Presentation` already has a `UserSecretsId`. Set each key from that project directory; nested keys use `:`.

```bash
cd Src/Services/WebApi/WebApi.Presentation
dotnet user-secrets set "BlobStorage:ServiceUrl" "https://..."
dotnet user-secrets set "BlobStorage:SecretKey" "..."
# verify:
dotnet user-secrets list | grep BlobStorage
```

## 5. Real values — `.env` (Docker Compose)

`compose.yaml` loads `.env` (git-ignored) into the container. Use the **`__` (double-underscore)** convention for nested keys, matching the existing `ConnectionStrings__CoreDB` / `Authentication__Swagger__ClientId` entries.

```
BlobStorage__ServiceUrl=https://...
BlobStorage__SecretKey=...
```

## Notes

- `appsettings.json` < user-secrets / environment — the empty placeholders are overridden at runtime, so only the values that differ need to be set in each store.
- Don't override what already has a good code default (e.g. a `ForcePathStyle=true` or a 5-minute expiry) unless the environment needs a different value.
- `dotnet build` after adding the class — warnings are errors.
- For a brand-new library that *introduces* such a section, scaffold the project with `add-integration` first; this skill covers the config wiring within it.
