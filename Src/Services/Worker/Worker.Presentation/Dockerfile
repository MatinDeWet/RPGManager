FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Src/Services/Worker/Worker.Presentation/Worker.Presentation.csproj", "Src/Services/Worker/Worker.Presentation/"]
COPY ["Src/Services/Worker/Worker.infrastructure/Worker.infrastructure.csproj", "Src/Services/Worker/Worker.infrastructure/"]
COPY ["Src/Services/Worker/Worker.Application/Worker.Application.csproj", "Src/Services/Worker/Worker.Application/"]
COPY ["Src/Services/Worker/Worker.Domain/Worker.Domain.csproj", "Src/Services/Worker/Worker.Domain/"]
COPY ["Src/Shared/Shared.Domain/Shared.Domain.csproj", "Src/Shared/Shared.Domain/"]
COPY ["Src/Shared/Shared.Persistence/Shared.Persistence.csproj", "Src/Shared/Shared.Persistence/"]
COPY ["Src/Utilities/Domain/Domain.csproj", "Src/Utilities/Domain/"]
COPY ["Src/Utilities/Repository/Repository.csproj", "Src/Utilities/Repository/"]
COPY ["Src/Utilities/Identification/Identification.csproj", "Src/Utilities/Identification/"]
RUN dotnet restore "Src/Services/Worker/Worker.Presentation/Worker.Presentation.csproj"
COPY . .
WORKDIR "/src/Src/Services/Worker/Worker.Presentation"
RUN dotnet build "./Worker.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Worker.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Worker.Presentation.dll"]
