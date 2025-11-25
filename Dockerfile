# Build stage for Vue.js frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app/client
COPY client-app/package*.json ./
RUN npm ci
COPY client-app/ ./
RUN npm run build

# Build stage for .NET backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY src/Contabilita.Core/*.csproj Contabilita.Core/
COPY src/Contabilita.Infrastructure/*.csproj Contabilita.Infrastructure/
COPY src/Contabilita.API/*.csproj Contabilita.API/
RUN dotnet restore Contabilita.API/Contabilita.API.csproj
COPY src/ ./
RUN dotnet publish Contabilita.API/Contabilita.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=backend-build /app/publish .
COPY --from=frontend-build /app/client/dist ./wwwroot
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 80
ENTRYPOINT ["dotnet", "Contabilita.API.dll"]
