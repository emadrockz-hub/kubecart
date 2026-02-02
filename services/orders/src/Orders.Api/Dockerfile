# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["services/orders/src/Orders.Api/Orders.Api.csproj", "services/orders/src/Orders.Api/"]
RUN dotnet restore "services/orders/src/Orders.Api/Orders.Api.csproj"

COPY . .
RUN dotnet publish "services/orders/src/Orders.Api/Orders.Api.csproj" -c Release -o /app/publish

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
