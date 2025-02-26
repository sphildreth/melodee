using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddSpotifyUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCustomImage",
                table: "Playlists");

            migrationBuilder.CreateIndex(
                name: "IX_Songs_SpotifyId",
                table: "Songs",
                column: "SpotifyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_SpotifyId",
                table: "Bookmarks",
                column: "SpotifyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artists_SpotifyId",
                table: "Artists",
                column: "SpotifyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_SpotifyId",
                table: "Albums",
                column: "SpotifyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Songs_SpotifyId",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Bookmarks_SpotifyId",
                table: "Bookmarks");

            migrationBuilder.DropIndex(
                name: "IX_Artists_SpotifyId",
                table: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_Albums_SpotifyId",
                table: "Albums");

            migrationBuilder.AddColumn<bool>(
                name: "HasCustomImage",
                table: "Playlists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

        }
    }
}
