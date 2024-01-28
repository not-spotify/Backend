FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY MusicPlayerBackend.sln .
COPY Directory.Packages.props .
COPY NuGet.Config .
COPY Common/*.csproj ./Common/
COPY Data/*.csproj ./Data/
COPY MusicPlayerBackend/*.csproj ./MusicPlayerBackend/
COPY Services/*.csproj ./Services/
COPY Tests/*.csproj ./Tests/

RUN dotnet restore

COPY Common/. ./Common/
COPY Data/. ./Data/
COPY MusicPlayerBackend/. ./MusicPlayerBackend/
COPY Services/. ./Services/

WORKDIR /source/MusicPlayerBackend
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "MusicPlayerBackend.dll"]
