using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedLikedPlaylist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Playlists_OwnerUserId",
                table: "Playlists");

            migrationBuilder.AddColumn<Guid>(
                name: "FavoriteTracksPlaylistId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_OwnerUserId",
                table: "Playlists",
                column: "OwnerUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Playlists_OwnerUserId",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "FavoriteTracksPlaylistId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_OwnerUserId",
                table: "Playlists",
                column: "OwnerUserId");
        }
    }
}
