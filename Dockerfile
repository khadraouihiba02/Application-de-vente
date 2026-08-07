# Étape 1: Image de base pour l'exécution (Runtime ASP.NET Core 8.0)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Étape 2: Image pour la compilation (SDK .NET 8.0)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers .csproj et restaurer les dépendances NuGet
COPY ["Application de vente/Application de vente.csproj", "Application de vente/"]
COPY ["Application de vente.Tests/Application de vente.Tests.csproj", "Application de vente.Tests/"]
RUN dotnet restore "Application de vente/Application de vente.csproj"

# Copier l'intégralité du code source
COPY . .

# Exécuter les tests unitaires pendant le build Docker
FROM build AS test
WORKDIR /src/Application de vente.Tests
RUN dotnet test --no-restore --verbosity normal

# Compiler le projet principal
FROM build AS publish
WORKDIR "/src/Application de vente"
RUN dotnet publish "Application de vente.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Étape 3: Image finale de production
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Application de vente.dll"]
