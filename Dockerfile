# Build Stage: Usa l'SDK di .NET 10 per compilare
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia il csproj e fa il restore delle dipendenze
COPY Backend.csproj ./
RUN dotnet restore "Backend.csproj"

# Copia tutto il resto del codice
COPY . .

# Pubblica l'applicazione
RUN dotnet publish "Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage: Usa l'immagine leggera di solo runtime per far girare l'app
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Se hai un database SQLite locale iniziale, potresti volerlo copiare 
# (Ma attenzione: su Render senza un disco persistente i dati si resettano ad ogni deploy)
# Se vuoi includere il db iniziale scommenta la riga sotto:
COPY --from=build /src/app.db .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://0.0.0.0:5000

ENTRYPOINT ["dotnet", "Backend.dll"]