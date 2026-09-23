# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo el .csproj primero (cache de capas Docker)
COPY ["Kiosco.Api.csproj", "./"]
RUN dotnet restore "Kiosco.Api.csproj"

# Copiar el resto del código y publicar
COPY . ./
RUN dotnet publish "Kiosco.Api.csproj" -c Release -o /app/publish --no-restore

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto (Render asigna PORT dinámicamente)
EXPOSE 5000

ENTRYPOINT ["dotnet", "Kiosco.Api.dll"]