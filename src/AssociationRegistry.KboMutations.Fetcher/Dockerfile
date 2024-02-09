FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AssociationRegistry.KboMutations.Fetcher/AssociationRegistry.KboMutations.Fetcher.csproj", "src/AssociationRegistry.KboMutations.Fetcher/"]
RUN dotnet restore "src/AssociationRegistry.KboMutations.Fetcher/AssociationRegistry.KboMutations.Fetcher.csproj"
COPY . .
WORKDIR "/src/src/AssociationRegistry.KboMutations.Fetcher"
RUN dotnet build "AssociationRegistry.KboMutations.Fetcher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AssociationRegistry.KboMutations.Fetcher.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AssociationRegistry.KboMutations.Fetcher.dll"]
