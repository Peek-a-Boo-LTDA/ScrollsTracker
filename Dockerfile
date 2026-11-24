FROM node:22 AS node-builder
WORKDIR /src

COPY ScrollsTracker.WebApp/package*.json ./
RUN npm ci

COPY ScrollsTracker.WebApp/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-builder
WORKDIR /src

COPY . ./
RUN dotnet publish ScrollsTracker.Api/ScrollsTracker.Api.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=dotnet-builder /publish ./
COPY --from=node-builder /src/dist ./wwwroot

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ScrollsTracker.Api.dll"]
