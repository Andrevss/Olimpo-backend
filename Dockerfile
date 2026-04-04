# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy source and restore
COPY . .
RUN dotnet restore "Olimpo.ProductAPI/Olimpo.ProductAPI.csproj"

# Publish API
RUN dotnet publish "Olimpo.ProductAPI/Olimpo.ProductAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render sets PORT dynamically
EXPOSE 10000
ENTRYPOINT ["sh", "-c", "dotnet Olimpo.ProductAPI.dll --urls http://0.0.0.0:${PORT:-10000}"]
