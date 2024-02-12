﻿// <auto-generated />
namespace MusicPlayerBackend.Persistence.Migrations

open System
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Infrastructure
open Microsoft.EntityFrameworkCore.Metadata
open Microsoft.EntityFrameworkCore.Migrations
open Microsoft.EntityFrameworkCore.Storage.ValueConversion
open MusicPlayerBackend.Persistence

[<DbContext(typeof<FsharpAppDbContext>)>]
[<Migration("20240212004213_Initial")>]
type Initial() =
    inherit Migration()

    override this.Up(migrationBuilder:MigrationBuilder) =
        migrationBuilder.CreateTable(
            name = "Albums"
            ,columns = (fun table -> 
            {|
                Id =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                CoverUri =
                    table.Column<string>(
                        nullable = true
                        ,``type`` = "text"
                    )
                CreatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = false
                        ,``type`` = "timestamp with time zone"
                    )
                UpdatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = true
                        ,``type`` = "timestamp with time zone"
                    )
            |})
            , constraints =
                (fun table -> 
                    table.PrimaryKey("PK_Albums", (fun x -> (x.Id) :> obj)
                    ) |> ignore
                )
        ) |> ignore

        migrationBuilder.CreateTable(
            name = "Users"
            ,columns = (fun table -> 
            {|
                Id =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                UserName =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "text"
                    )
                NormalizedUserName =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "text"
                    )
                Email =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "text"
                    )
                NormalizedEmail =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "text"
                    )
                HashedPassword =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "text"
                    )
                FavoritePlaylistId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                CreatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = false
                        ,``type`` = "timestamp with time zone"
                    )
                UpdatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = true
                        ,``type`` = "timestamp with time zone"
                    )
            |})
            , constraints =
                (fun table -> 
                    table.PrimaryKey("PK_Users", (fun x -> (x.Id) :> obj)
                    ) |> ignore
                )
        ) |> ignore

        migrationBuilder.CreateTable(
            name = "Playlists"
            ,columns = (fun table -> 
            {|
                Id =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                Name =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "text"
                    )
                Visibility =
                    table.Column<int>(
                        nullable = false
                        ,``type`` = "integer"
                    )
                CoverUri =
                    table.Column<string>(
                        nullable = true
                        ,``type`` = "text"
                    )
                OwnerUserId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                CreatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = false
                        ,``type`` = "timestamp with time zone"
                    )
                UpdatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = true
                        ,``type`` = "timestamp with time zone"
                    )
            |})
            , constraints =
                (fun table -> 
                    table.PrimaryKey("PK_Playlists", (fun x -> (x.Id) :> obj)
                    ) |> ignore
                    table.ForeignKey(
                        name = "FK_Playlists_Users_OwnerUserId"
                        ,column = (fun x -> (x.OwnerUserId) :> obj)
                        ,principalTable = "Users"
                        ,principalColumn = "Id"
                        ,onDelete = ReferentialAction.Cascade
                        ) |> ignore

                )
        ) |> ignore

        migrationBuilder.CreateTable(
            name = "RefreshTokens"
            ,columns = (fun table -> 
            {|
                Id =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                Jti =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                Token =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                Revoked =
                    table.Column<bool>(
                        nullable = false
                        ,``type`` = "boolean"
                    )
                UserId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                ValidDue =
                    table.Column<DateTimeOffset>(
                        nullable = false
                        ,``type`` = "timestamp with time zone"
                    )
                CreatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = false
                        ,``type`` = "timestamp with time zone"
                    )
                UpdatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = true
                        ,``type`` = "timestamp with time zone"
                    )
            |})
            , constraints =
                (fun table -> 
                    table.PrimaryKey("PK_RefreshTokens", (fun x -> (x.Id) :> obj)
                    ) |> ignore
                    table.ForeignKey(
                        name = "FK_RefreshTokens_Users_UserId"
                        ,column = (fun x -> (x.UserId) :> obj)
                        ,principalTable = "Users"
                        ,principalColumn = "Id"
                        ,onDelete = ReferentialAction.Cascade
                        ) |> ignore

                )
        ) |> ignore

        migrationBuilder.CreateTable(
            name = "Tracks"
            ,columns = (fun table -> 
            {|
                Id =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                OwnerUserId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                CoverUri =
                    table.Column<string>(
                        nullable = true
                        ,``type`` = "text"
                    )
                TrackUri =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "text"
                    )
                Visibility =
                    table.Column<int>(
                        nullable = false
                        ,``type`` = "integer"
                    )
                Name =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "text"
                    )
                Author =
                    table.Column<string>(
                        nullable = false
                        ,``type`` = "text"
                    )
                CreatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = false
                        ,``type`` = "timestamp with time zone"
                    )
                UpdatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = true
                        ,``type`` = "timestamp with time zone"
                    )
            |})
            , constraints =
                (fun table -> 
                    table.PrimaryKey("PK_Tracks", (fun x -> (x.Id) :> obj)
                    ) |> ignore
                    table.ForeignKey(
                        name = "FK_Tracks_Users_OwnerUserId"
                        ,column = (fun x -> (x.OwnerUserId) :> obj)
                        ,principalTable = "Users"
                        ,principalColumn = "Id"
                        ,onDelete = ReferentialAction.Cascade
                        ) |> ignore

                )
        ) |> ignore

        migrationBuilder.CreateTable(
            name = "PlaylistUserPermissions"
            ,columns = (fun table -> 
            {|
                PlaylistId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                UserId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                Permission =
                    table.Column<int>(
                        nullable = false
                        ,``type`` = "integer"
                    )
                CreatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = false
                        ,``type`` = "timestamp with time zone"
                    )
                UpdatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = true
                        ,``type`` = "timestamp with time zone"
                    )
            |})
            , constraints =
                (fun table -> 
                    table.PrimaryKey("PK_PlaylistUserPermissions", (fun x -> (x.PlaylistId, x.UserId, x.Permission) :> obj)
                    ) |> ignore
                    table.ForeignKey(
                        name = "FK_PlaylistUserPermissions_Playlists_PlaylistId"
                        ,column = (fun x -> (x.PlaylistId) :> obj)
                        ,principalTable = "Playlists"
                        ,principalColumn = "Id"
                        ,onDelete = ReferentialAction.Cascade
                        ) |> ignore

                    table.ForeignKey(
                        name = "FK_PlaylistUserPermissions_Users_UserId"
                        ,column = (fun x -> (x.UserId) :> obj)
                        ,principalTable = "Users"
                        ,principalColumn = "Id"
                        ,onDelete = ReferentialAction.Cascade
                        ) |> ignore

                )
        ) |> ignore

        migrationBuilder.CreateTable(
            name = "AlbumTracks"
            ,columns = (fun table -> 
            {|
                AlbumId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                TrackId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                CreatedAt =
                    table.Column<DateTimeOffset>(
                        nullable = false
                        ,``type`` = "timestamp with time zone"
                    )
            |})
            , constraints =
                (fun table -> 
                    table.PrimaryKey("PK_AlbumTracks", (fun x -> (x.AlbumId, x.TrackId) :> obj)
                    ) |> ignore
                    table.ForeignKey(
                        name = "FK_AlbumTracks_Albums_AlbumId"
                        ,column = (fun x -> (x.AlbumId) :> obj)
                        ,principalTable = "Albums"
                        ,principalColumn = "Id"
                        ,onDelete = ReferentialAction.Cascade
                        ) |> ignore

                    table.ForeignKey(
                        name = "FK_AlbumTracks_Tracks_TrackId"
                        ,column = (fun x -> (x.TrackId) :> obj)
                        ,principalTable = "Tracks"
                        ,principalColumn = "Id"
                        ,onDelete = ReferentialAction.Cascade
                        ) |> ignore

                )
        ) |> ignore

        migrationBuilder.CreateTable(
            name = "TrackPlaylist"
            ,columns = (fun table -> 
            {|
                TrackId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                PlaylistId =
                    table.Column<Guid>(
                        nullable = false
                        ,``type`` = "uuid"
                    )
                AddedAt =
                    table.Column<DateTimeOffset>(
                        nullable = false
                        ,``type`` = "timestamp with time zone"
                    )
            |})
            , constraints =
                (fun table -> 
                    table.PrimaryKey("PK_TrackPlaylist", (fun x -> (x.TrackId, x.PlaylistId) :> obj)
                    ) |> ignore
                    table.ForeignKey(
                        name = "FK_TrackPlaylist_Playlists_PlaylistId"
                        ,column = (fun x -> (x.PlaylistId) :> obj)
                        ,principalTable = "Playlists"
                        ,principalColumn = "Id"
                        ,onDelete = ReferentialAction.Cascade
                        ) |> ignore

                    table.ForeignKey(
                        name = "FK_TrackPlaylist_Tracks_TrackId"
                        ,column = (fun x -> (x.TrackId) :> obj)
                        ,principalTable = "Tracks"
                        ,principalColumn = "Id"
                        ,onDelete = ReferentialAction.Cascade
                        ) |> ignore

                )
        ) |> ignore

        migrationBuilder.CreateIndex(
            name = "IX_AlbumTracks_TrackId"
            ,table = "AlbumTracks"
            ,column = "TrackId"
            ) |> ignore

        migrationBuilder.CreateIndex(
            name = "IX_Playlists_OwnerUserId"
            ,table = "Playlists"
            ,column = "OwnerUserId"
            ,unique = true
            ) |> ignore

        migrationBuilder.CreateIndex(
            name = "IX_PlaylistUserPermissions_UserId"
            ,table = "PlaylistUserPermissions"
            ,column = "UserId"
            ) |> ignore

        migrationBuilder.CreateIndex(
            name = "IX_RefreshTokens_UserId"
            ,table = "RefreshTokens"
            ,column = "UserId"
            ) |> ignore

        migrationBuilder.CreateIndex(
            name = "IX_TrackPlaylist_PlaylistId"
            ,table = "TrackPlaylist"
            ,column = "PlaylistId"
            ) |> ignore

        migrationBuilder.CreateIndex(
            name = "IX_Tracks_OwnerUserId"
            ,table = "Tracks"
            ,column = "OwnerUserId"
            ) |> ignore

        migrationBuilder.CreateIndex(
            name = "IX_Users_NormalizedEmail"
            ,table = "Users"
            ,column = "NormalizedEmail"
            ,unique = true
            ) |> ignore

        migrationBuilder.CreateIndex(
            name = "IX_Users_NormalizedUserName"
            ,table = "Users"
            ,column = "NormalizedUserName"
            ,unique = true
            ) |> ignore


    override this.Down(migrationBuilder:MigrationBuilder) =
        migrationBuilder.DropTable(
            name = "AlbumTracks"
            ) |> ignore

        migrationBuilder.DropTable(
            name = "PlaylistUserPermissions"
            ) |> ignore

        migrationBuilder.DropTable(
            name = "RefreshTokens"
            ) |> ignore

        migrationBuilder.DropTable(
            name = "TrackPlaylist"
            ) |> ignore

        migrationBuilder.DropTable(
            name = "Albums"
            ) |> ignore

        migrationBuilder.DropTable(
            name = "Playlists"
            ) |> ignore

        migrationBuilder.DropTable(
            name = "Tracks"
            ) |> ignore

        migrationBuilder.DropTable(
            name = "Users"
            ) |> ignore


    override this.BuildTargetModel(modelBuilder: ModelBuilder) =
        modelBuilder
            .HasAnnotation("ProductVersion", "6.0.26")
            .HasAnnotation("Relational:MaxIdentifierLength", 63) |> ignore

        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.Album", (fun b ->

            b.Property<Guid>("Id")
                .IsRequired(true)
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                |> ignore

            b.Property<string option>("CoverUri")
                .IsRequired(false)
                .HasColumnType("text")
                |> ignore

            b.Property<DateTimeOffset>("CreatedAt")
                .IsRequired(true)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<DateTimeOffset option>("UpdatedAt")
                .IsRequired(false)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.HasKey("Id")
                |> ignore


            b.ToTable("Albums") |> ignore

        )) |> ignore

        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.AlbumTrack", (fun b ->

            b.Property<Guid>("AlbumId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<Guid>("TrackId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<DateTimeOffset>("CreatedAt")
                .IsRequired(true)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.HasKey("AlbumId", "TrackId")
                |> ignore


            b.HasIndex("TrackId")
                |> ignore

            b.ToTable("AlbumTracks") |> ignore

        )) |> ignore

        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.Playlist", (fun b ->

            b.Property<Guid>("Id")
                .IsRequired(true)
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                |> ignore

            b.Property<string option>("CoverUri")
                .IsRequired(false)
                .HasColumnType("text")
                |> ignore

            b.Property<DateTimeOffset>("CreatedAt")
                .IsRequired(true)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<string>("Name")
                .IsRequired(true)
                .HasColumnType("text")
                |> ignore

            b.Property<Guid>("OwnerUserId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<DateTimeOffset option>("UpdatedAt")
                .IsRequired(false)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<int>("Visibility")
                .IsRequired(true)
                .HasColumnType("integer")
                |> ignore

            b.HasKey("Id")
                |> ignore


            b.HasIndex("OwnerUserId")
                .IsUnique()
                |> ignore

            b.ToTable("Playlists") |> ignore

        )) |> ignore

        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.PlaylistUserPermission", (fun b ->

            b.Property<Guid>("PlaylistId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<Guid>("UserId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<int>("Permission")
                .IsRequired(true)
                .HasColumnType("integer")
                |> ignore

            b.Property<DateTimeOffset>("CreatedAt")
                .IsRequired(true)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<DateTimeOffset option>("UpdatedAt")
                .IsRequired(false)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.HasKey("PlaylistId", "UserId", "Permission")
                |> ignore


            b.HasIndex("UserId")
                |> ignore

            b.ToTable("PlaylistUserPermissions") |> ignore

        )) |> ignore

        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.RefreshToken", (fun b ->

            b.Property<Guid>("Id")
                .IsRequired(true)
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                |> ignore

            b.Property<DateTimeOffset>("CreatedAt")
                .IsRequired(true)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<Guid>("Jti")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<bool>("Revoked")
                .IsRequired(true)
                .HasColumnType("boolean")
                |> ignore

            b.Property<Guid>("Token")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<DateTimeOffset option>("UpdatedAt")
                .IsRequired(false)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<Guid>("UserId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<DateTimeOffset>("ValidDue")
                .IsRequired(true)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.HasKey("Id")
                |> ignore


            b.HasIndex("UserId")
                |> ignore

            b.ToTable("RefreshTokens") |> ignore

        )) |> ignore

        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.Track", (fun b ->

            b.Property<Guid>("Id")
                .IsRequired(true)
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                |> ignore

            b.Property<string>("Author")
                .IsRequired(true)
                .HasColumnType("text")
                |> ignore

            b.Property<string option>("CoverUri")
                .IsRequired(false)
                .HasColumnType("text")
                |> ignore

            b.Property<DateTimeOffset>("CreatedAt")
                .IsRequired(true)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<string>("Name")
                .IsRequired(true)
                .HasColumnType("text")
                |> ignore

            b.Property<Guid>("OwnerUserId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<string>("TrackUri")
                .IsRequired(true)
                .HasColumnType("text")
                |> ignore

            b.Property<DateTimeOffset option>("UpdatedAt")
                .IsRequired(false)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<int>("Visibility")
                .IsRequired(true)
                .HasColumnType("integer")
                |> ignore

            b.HasKey("Id")
                |> ignore


            b.HasIndex("OwnerUserId")
                |> ignore

            b.ToTable("Tracks") |> ignore

        )) |> ignore

        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.TrackPlaylist", (fun b ->

            b.Property<Guid>("TrackId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<Guid>("PlaylistId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<DateTimeOffset>("AddedAt")
                .IsRequired(true)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.HasKey("TrackId", "PlaylistId")
                |> ignore


            b.HasIndex("PlaylistId")
                |> ignore

            b.ToTable("TrackPlaylist") |> ignore

        )) |> ignore

        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.User", (fun b ->

            b.Property<Guid>("Id")
                .IsRequired(true)
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid")
                |> ignore

            b.Property<DateTimeOffset>("CreatedAt")
                .IsRequired(true)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<string>("Email")
                .IsRequired(true)
                .HasColumnType("text")
                |> ignore

            b.Property<Guid>("FavoritePlaylistId")
                .IsRequired(true)
                .HasColumnType("uuid")
                |> ignore

            b.Property<string>("HashedPassword")
                .IsRequired(true)
                .HasColumnType("text")
                |> ignore

            b.Property<string>("NormalizedEmail")
                .IsRequired(true)
                .HasColumnType("text")
                |> ignore

            b.Property<string>("NormalizedUserName")
                .IsRequired(true)
                .HasColumnType("text")
                |> ignore

            b.Property<DateTimeOffset option>("UpdatedAt")
                .IsRequired(false)
                .HasColumnType("timestamp with time zone")
                |> ignore

            b.Property<string>("UserName")
                .IsRequired(true)
                .HasColumnType("text")
                |> ignore

            b.HasKey("Id")
                |> ignore


            b.HasIndex("NormalizedEmail")
                .IsUnique()
                |> ignore


            b.HasIndex("NormalizedUserName")
                .IsUnique()
                |> ignore

            b.ToTable("Users") |> ignore

        )) |> ignore
        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.AlbumTrack", (fun b ->
            b.HasOne("MusicPlayerBackend.Persistence.Entities.Album", "Album")
                .WithMany("AlbumTracks")
                .HasForeignKey("AlbumId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore
            b.HasOne("MusicPlayerBackend.Persistence.Entities.Track", "Track")
                .WithMany()
                .HasForeignKey("TrackId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore

        )) |> ignore
        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.Playlist", (fun b ->
            b.HasOne("MusicPlayerBackend.Persistence.Entities.User", "OwnerUser")
                .WithOne("FavoritePlaylist")
                .HasForeignKey("MusicPlayerBackend.Persistence.Entities.Playlist", "OwnerUserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore

        )) |> ignore
        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.PlaylistUserPermission", (fun b ->
            b.HasOne("MusicPlayerBackend.Persistence.Entities.Playlist", "Playlist")
                .WithMany()
                .HasForeignKey("PlaylistId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore
            b.HasOne("MusicPlayerBackend.Persistence.Entities.User", "User")
                .WithMany("Permissions")
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore

        )) |> ignore
        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.RefreshToken", (fun b ->
            b.HasOne("MusicPlayerBackend.Persistence.Entities.User", "User")
                .WithMany()
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore

        )) |> ignore
        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.Track", (fun b ->
            b.HasOne("MusicPlayerBackend.Persistence.Entities.User", "OwnerUser")
                .WithMany()
                .HasForeignKey("OwnerUserId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore

        )) |> ignore
        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.TrackPlaylist", (fun b ->
            b.HasOne("MusicPlayerBackend.Persistence.Entities.Playlist", "Playlist")
                .WithMany("TrackPlaylists")
                .HasForeignKey("PlaylistId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore
            b.HasOne("MusicPlayerBackend.Persistence.Entities.Track", "Track")
                .WithMany()
                .HasForeignKey("TrackId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired()
                |> ignore

        )) |> ignore
        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.Album", (fun b ->

            b.Navigation("AlbumTracks")
            |> ignore
        )) |> ignore
        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.Playlist", (fun b ->

            b.Navigation("TrackPlaylists")
            |> ignore
        )) |> ignore
        modelBuilder.Entity("MusicPlayerBackend.Persistence.Entities.User", (fun b ->

            b.Navigation("FavoritePlaylist")
            |> ignore

            b.Navigation("Permissions")
            |> ignore
        )) |> ignore

