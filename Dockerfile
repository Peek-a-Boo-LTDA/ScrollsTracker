FROM node:20 AS node-builder
WORKDIR /src

# 1. Copia o package.json de DENTRO da pasta ScrollsTracker.WebApp para a raiz do estágio de build
# ATENÇÃO: Estou assumindo que seu front está nessa pasta baseado no seu print anterior
COPY ScrollsTracker.WebApp/package*.json ./

# 2. Instala as dependências
RUN npm ci

# 3. Copia o restante dos arquivos DA PASTA WebApp para o container
COPY ScrollsTracker.WebApp/ ./

# 4. Roda o build do Vite
RUN npm run build


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-builder
WORKDIR /app

COPY . ./

RUN dotnet publish "ScrollsTracker.Api/ScrollsTracker.Api.csproj" -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=dotnet-builder /app/publish .

COPY --from=node-builder /src/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ScrollsTracker.Api.dll"]