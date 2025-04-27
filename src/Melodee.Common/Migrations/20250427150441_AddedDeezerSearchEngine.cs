using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeezerSearchEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeezerId",
                table: "Songs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeezerId",
                table: "Bookmarks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeezerId",
                table: "Artists",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeezerId",
                table: "Albums",
                type: "integer",
                nullable: true);
            
            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 918, new Guid("6efc0442-0d11-435e-b17c-9dd89a1006fa"), 9, "Is Deezer search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17457662807598984L), null, false, "searchEngine.deezer.enabled", null, null, 0, null, "true" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918);

            migrationBuilder.DropColumn(
                name: "DeezerId",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "DeezerId",
                table: "Bookmarks");

            migrationBuilder.DropColumn(
                name: "DeezerId",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "DeezerId",
                table: "Albums");

        }
    }
}
