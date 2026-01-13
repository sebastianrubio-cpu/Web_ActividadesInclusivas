# USAMOS LA IMAGEN DE CONSTRUCCIÓN (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EstudiantesAPI.csproj", "./"]
RUN dotnet restore "EstudiantesAPI.csproj"
COPY . .
RUN dotnet publish "EstudiantesAPI.csproj" -c Release -o /app/publish

# USAMOS LA IMAGEN FINAL (RUNTIME LIGERO)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EstudiantesAPI.dll"]