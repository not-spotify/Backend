# MusicPlayer Backend

## How to start project

* Start storages compose

```bash
cd Infr/Storage/
docker compose up -d
```

* Strart backend

```bash
cd ../..
cd Infr/Backend
docker compose up -d
```

* Open Swagger

[Swagger](http://localhost:9780/swagger/index.html)

## How to create migration

```bash
dotnet ef migrations add <Name> --startup-project MusicPlayerBackend --project Data
```

