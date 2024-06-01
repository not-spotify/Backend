FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

COPY . ./
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 as exec-env
WORKDIR /App
COPY --from=build-env /App/out .

ENTRYPOINT dotnet Host.dll --urls "http://0.0.0.0:9780"