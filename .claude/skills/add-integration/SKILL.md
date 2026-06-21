---
name: add-integration
description: Scaffold a new integration/utility class library in the RPGManager solution (e.g. an external-service client under Src/Integrations or a shared library under Src/Utilities). Use when adding a whole new project — not a feature inside an existing one (e.g. "add a BlobStorage integration", "add an email-sending integration", "create a new Utilities library"). Creates the project with central build/package conventions, the options + contracts + implementation + DI-extension layout, registers it in RPGManager.slnx, and wires it into the consuming project.
---

# Add an integration / utility library

A self-contained class library that wraps an external service (SeaweedFS, an email provider, …) or shares cross-cutting code. Integrations live under `Src/Integrations/`; general building blocks live under `Src/Utilities/`. Both follow the same project shape — model it on `Src/Utilities/Caching` and the existing `Src/Integrations/BlobStorage.Integration`.

Layout (sub-namespaces match folders, file-scoped namespaces throughout):

```
Src/Integrations/<Name>.Integration/
  <Name>.Integration.csproj
  <Name>DI.cs                          // namespace <Name>; — public AddX extension
  Configuration/<Name>Options.cs       // sealed; const SectionName
  Contracts/I<Name>Service.cs          // the public abstraction
  Contracts/Models/*.cs                // sealed record DTOs
  Implementation/<Name>Service.cs      // internal sealed; implements the contract
```

## 1. Project file — `Src/Integrations/<Name>.Integration/<Name>.Integration.csproj`

- **Do NOT** declare `TargetFramework`/`Nullable`/`ImplicitUsings` — inherited from `Directory.Build.props` (and `TreatWarningsAsErrors=true`, so the code must be analyzer-clean).
- Override `RootNamespace` so a dotted assembly name (`BlobStorage.Integration`) yields a clean root namespace (`namespace BlobStorage;`).
- Reference packages with **no `Version`** — versions are centrally managed.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>BlobStorage</RootNamespace>
    <AssemblyName>BlobStorage.Integration</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AWSSDK.S3" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" />
  </ItemGroup>

</Project>
```

The three `Microsoft.Extensions.*.Abstractions`/`Options.ConfigurationExtensions` references are what every options + DI-extension library needs (same set `Caching.csproj` uses).

## 2. Central package management — `Directory.Packages.props`

Any **new** NuGet needs a `<PackageVersion Include="X" Version="N" />` line in the root `Directory.Packages.props` `<ItemGroup>` first (transitive deps don't — e.g. `AWSSDK.Core` rides along with `AWSSDK.S3`). The csproj `<PackageReference>` then omits the version. Without this, restore fails under central package management.

## 3. Options — `Configuration/<Name>Options.cs`

`sealed`, a `public const string SectionName`, sensible defaults. Hold **connection/config only** — values that vary per call (a bucket, a recipient) are method parameters, not options. Bound in DI via `services.Configure<T>(...)` (see `add-options` for the full options pattern, secrets, and `.env`).

```csharp
namespace BlobStorage.Configuration;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public string ServiceUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public TimeSpan PresignedUrlExpiry { get; set; } = TimeSpan.FromMinutes(5);
}
```

## 4. Contract + DTOs — `Contracts/`

- `I<Name>Service` is the only public surface besides the options and `AddX`. Async methods take `CancellationToken cancellationToken = default`; pure local operations stay synchronous.
- DTOs are `public sealed record` under `Contracts/Models/`.
- **Analyzer gotchas (warnings = errors):** expose URLs as `Uri`, not `string` (CA1056). If a method returns a stream the caller must dispose, document it and suppress CA2000 on the method with a justification (`[SuppressMessage("Reliability", "CA2000", Justification = "Stream ownership transferred to caller via <Dto>.")]`).

## 5. Implementation — `Implementation/<Name>Service.cs`

`internal sealed`, implements the contract, injects the client + `IOptions<<Name>Options>` (read `.Value` once in the ctor). Validate args with `ArgumentException.ThrowIfNullOrEmpty(...)` / `ArgumentNullException.ThrowIfNull(...)` (avoid pulling in `Ardalis.GuardClauses` just for this).

## 6. DI extension — `<Name>DI.cs`

`namespace <Name>;` (root), a `public static IServiceCollection Add<Name>(this IServiceCollection services, IConfiguration configuration)` that binds options, registers the client, and registers the service.

```csharp
namespace BlobStorage;

public static class BlobStorageDI
{
    public static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BlobStorageOptions>(configuration.GetSection(BlobStorageOptions.SectionName));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            BlobStorageOptions options = sp.GetRequiredService<IOptions<BlobStorageOptions>>().Value;
            // build the client from options...
        });

        services.AddSingleton<IBlobStorageService, SeaweedBlobStorageService>();
        return services;
    }
}
```

Stateless, thread-safe clients/services are `AddSingleton` (matches `ICacheService`); use `AddScoped` only if the service holds per-request state.

## 7. Register in the solution — `RPGManager.slnx`

Add the project under its area folder (create the `/Src/Integrations/` folder node if it's the first integration). Keep the file's 2-space indentation:

```xml
<Folder Name="/Src/Integrations/">
  <Project Path="Src/Integrations/BlobStorage.Integration/BlobStorage.Integration.csproj" />
</Folder>
```

## 8. Wire it into the consumer

- **Reference the project** from wherever it's consumed. For an integration injected by use-cases, that's `WebApi.Application` (`<ProjectReference Include="..\..\..\Integrations\<Name>.Integration\<Name>.Integration.csproj" />`). It then resolves transitively at the composition root — no extra Presentation reference needed.
- **Call the extension** in `Src/Services/WebApi/WebApi.Presentation/Common/DIExtensions/ServiceCollectionExtensions.cs`: add `using <Name>;` and chain `services.Add<Name>(configuration);` in `AddApplicationServices` (alongside `AddCachingSupport`).
- **Add the config section** to `appsettings.json` with empty placeholders (matching the empty `Authentication`/`Cache` style); real values go in user-secrets / `.env` (see `add-options`).

## After scaffolding

`dotnet build` from the solution root — must be analyzer-clean (warnings are errors). Confirm any new package restored. There is no test project convention for integrations yet, so verification against the live service is manual.
