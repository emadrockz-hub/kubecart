# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["services/identity/src/Identity.Api/Identity.Api.csproj", "services/identity/src/Identity.Api/"]
RUN dotnet restore "services/identity/src/Identity.Api/Identity.Api.csproj"

# copy the rest and publish
COPY . .
RUN dotnet publish "services/identity/src/Identity.Api/Identity.Api.csproj" -c Release -o /app/publish

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Identity.Api.dll"]
