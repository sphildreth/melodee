using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedShareTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShareIds",
                table: "Shares");

            migrationBuilder.AddColumn<int>(
                name: "ShareId",
                table: "Shares",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShareType",
                table: "Shares",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ShareUniqueId",
                table: "Shares",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ShareActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ShareId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    ByUserAgent = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Client = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareActivities", x => x.Id);
                });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShareActivities");

            migrationBuilder.DropColumn(
                name: "ShareId",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "ShareType",
                table: "Shares");

            migrationBuilder.DropColumn(
                name: "ShareUniqueId",
                table: "Shares");

            migrationBuilder.AddColumn<string>(
                name: "ShareIds",
                table: "Shares",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

        }
    }
}
