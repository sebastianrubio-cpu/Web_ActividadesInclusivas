# USAMOS LA IMAGEN DE CONSTRUCCIÓN (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SistemaWeb.csproj", "./"]
RUN dotnet restore "SistemaWeb.csproj"
COPY . .
RUN dotnet publish "SistemaWeb.csproj" -c Release -o /app/publish

# USAMOS LA IMAGEN FINAL (RUNTIME LIGERO)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SistemaWeb.dll"]