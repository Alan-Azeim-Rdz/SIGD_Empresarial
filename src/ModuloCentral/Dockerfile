# Etapa base para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiamos el archivo protegiendo los espacios con comillas dobles
COPY ["Gestion de Documentos.csproj", "./"]
RUN dotnet restore "Gestion de Documentos.csproj"

# Copiar el resto del código y compilar
COPY . .
RUN dotnet build "Gestion de Documentos.csproj" -c Release -o /app/build

# Etapa de publicación
FROM build AS publish
RUN dotnet publish "Gestion de Documentos.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final (Imagen de producción)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ejecutar el DLL generado
ENTRYPOINT ["dotnet", "Gestion de Documentos.dll"]