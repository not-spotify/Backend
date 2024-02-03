FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY . .

RUN dotnet restore
ENTRYPOINT dotnet run --project MusicPlayerBackend.Host --urls "http://0.0.0.0:9780"
