# MusicPlayer Backend

## How to start the project

### Linux users

Please add this line to `/etc/hosts`

```shell
127.0.0.1   host.docker.internal
```

### Installation

* Start storages compose

```shell
cd Infr/Storage/
docker compose up -d
```

* Start backend

```shell
cd ../..
cd Infr/Backend
docker compose up -d
```

* Open Swagger

[Swagger](http://localhost:9780/swagger/index.html)

## How to create migration

```shell
dotnet ef migrations add <Name> --startup-project MusicPlayerBackend --project Data
```
