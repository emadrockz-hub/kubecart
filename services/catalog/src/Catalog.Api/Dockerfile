# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["services/catalog/src/Catalog.Api/Catalog.Api.csproj", "services/catalog/src/Catalog.Api/"]
RUN dotnet restore "services/catalog/src/Catalog.Api/Catalog.Api.csproj"

COPY . .
RUN dotnet publish "services/catalog/src/Catalog.Api/Catalog.Api.csproj" -c Release -o /app/publish

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
