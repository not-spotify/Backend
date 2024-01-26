using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamedToFavoritePlaylist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FavoriteTracksPlaylistId",
                table: "Users",
                newName: "FavoritePlaylistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FavoritePlaylistId",
                table: "Users",
                newName: "FavoriteTracksPlaylistId");
        }
    }
}
