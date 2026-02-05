# Etapa 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto y restaurar dependencias
COPY ["SistemaWeb.csproj", "./"]
RUN dotnet restore "SistemaWeb.csproj"

# Copiar todo el código fuente y compilar
COPY . .
RUN dotnet publish "SistemaWeb.csproj" -c Release -o /app/publish

# Etapa 2: Ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Copiar los binarios compilados desde la etapa anterior
COPY --from=build /app/publish .

# Crear carpeta para los datos JSON (Importante para permisos)
RUN mkdir -p /app/Data

ENTRYPOINT ["dotnet", "SistemaWeb.dll"]