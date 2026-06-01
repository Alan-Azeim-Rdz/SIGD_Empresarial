# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Gestion de Documentos/Gestion de Documentos.csproj", "Gestion de Documentos/"]
RUN dotnet restore "./Gestion de Documentos/Gestion de Documentos.csproj"
COPY . .
WORKDIR "/src/Gestion de Documentos"

# --- NUEVA ETAPA: DESARROLLO (Aquí ocurre la magia) ---
FROM build AS development
ENV DOTNET_USE_POLLING_FILE_WATCHER=1
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://+:8080", "--no-launch-profile"]

# --- ETAPAS ORIGINALES (Para Producción) ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Gestion de Documentos.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gestion de Documentos.dll"]