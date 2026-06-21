FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Src/Services/WebApi/WebApi.Presentation/WebApi.Presentation.csproj", "Src/Services/WebApi/WebApi.Presentation/"]
COPY ["Src/Services/WebApi/WebApi.infrastructure/WebApi.infrastructure.csproj", "Src/Services/WebApi/WebApi.infrastructure/"]
COPY ["Src/Services/WebApi/WebApi.Application/WebApi.Application.csproj", "Src/Services/WebApi/WebApi.Application/"]
COPY ["Src/Services/WebApi/WebApi.Domain/WebApi.Domain.csproj", "Src/Services/WebApi/WebApi.Domain/"]
COPY ["Src/Shared/Shared.Domain/Shared.Domain.csproj", "Src/Shared/Shared.Domain/"]
COPY ["Src/Shared/Shared.Persistence/Shared.Persistence.csproj", "Src/Shared/Shared.Persistence/"]
COPY ["Src/Utilities/Domain/Domain.csproj", "Src/Utilities/Domain/"]
COPY ["Src/Utilities/Repository/Repository.csproj", "Src/Utilities/Repository/"]
COPY ["Src/Utilities/Identification/Identification.csproj", "Src/Utilities/Identification/"]
COPY ["Src/Utilities/Caching/Caching.csproj", "Src/Utilities/Caching/"]
COPY ["Src/Utilities/CQRS/CQRS.Core/CQRS.Core.csproj", "Src/Utilities/CQRS/CQRS.Core/"]
COPY ["Src/Utilities/CQRS/CQRS.Domain/CQRS.Domain.csproj", "Src/Utilities/CQRS/CQRS.Domain/"]
COPY ["Src/Integrations/BlobStorage.Integration/BlobStorage.Integration.csproj", "Src/Integrations/BlobStorage.Integration/"]
RUN dotnet restore "Src/Services/WebApi/WebApi.Presentation/WebApi.Presentation.csproj"
COPY . .
WORKDIR "/src/Src/Services/WebApi/WebApi.Presentation"
RUN dotnet build "./WebApi.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WebApi.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApi.Presentation.dll"]
