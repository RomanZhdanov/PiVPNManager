# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app
EXPOSE 80
EXPOSE 443
    
# Copy csproj and restore as distinct layers
COPY *.sln .
COPY ./src/Application/*csproj ./src/Application/
COPY ./src/Domain/*csproj ./src/Domain/
COPY ./src/Infrastructure/*csproj ./src/Infrastructure/
COPY ./src/WebUI/*csproj ./src/WebUI/
RUN dotnet restore
    
# Copy everything else and build
COPY ./src/Application/. ./src/Application/
COPY ./src/Domain/. ./src/Domain/
COPY ./src/Infrastructure/. ./src/Infrastructure/
COPY ./src/WebUI/. ./src/WebUI/

WORKDIR /app/src/WebUI
RUN dotnet publish -c Release -o out
    
# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/src/WebUI/out .
ENTRYPOINT ["dotnet", "PiVPNManager.WebUI.dll"]

