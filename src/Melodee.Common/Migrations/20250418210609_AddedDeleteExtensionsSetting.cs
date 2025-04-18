using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeleteExtensionsSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 54, new Guid("99b37d11-cc14-497f-8b99-5ae46271e26f"), null, "When processing folders immediately delete any files with these extensions. (JSON array).", NodaTime.Instant.FromUnixTimeTicks(17450103681440965L), null, false, "processing.fileExtensionsToDelete", null, null, 0, null, "['log', 'lnk', 'lrc', 'doc']" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54);
        }
    }
}
