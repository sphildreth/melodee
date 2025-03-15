using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedDescriptionAndBiographyLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserSongs",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Users",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserPins",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserArtists",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserAlbums",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Songs",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Shares",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Settings",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "RadioStations",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PlayQues",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Playlists",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Players",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Libraries",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Contributors",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Bookmarks",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Artists",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Biography",
                table: "Artists",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ArtistRelation",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Albums",
                type: "character varying(62000)",
                maxLength: 62000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("30df25fb-6e99-4f5e-8d6d-62cc76c6a76c"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5d218e2d-1a3e-4f50-bf16-2bc570c4044a"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("50ba32c2-f8fc-4552-b9fc-fff63539c4e4"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ff564b2-60ba-4f51-b5a7-f48cb6413ea8"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cecc4897-1813-4a67-b31e-9c7eb4dabbf3"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8bdc8c35-cc08-4dc8-8dc9-d375bb462588"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b1b5fba6-44e4-4af3-b242-d88513d3ac97"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95685bb2-aeb7-40db-ac36-be24450505d2"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1dba6df9-6ace-46fa-a1de-6dbb2d4bdde9"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("20034810-8282-40ef-a7de-0b9d7532e350"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca15656b-8ffa-4e5e-a179-0987c7002587"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("81e3be25-c037-429b-9041-f57d6a1188fc"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2d52d3dc-63e5-435f-8494-ff3d25a0ffe1"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bf4ce5c0-8cdf-4c9e-9330-2d7def82c68c"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ccb42c0-ec60-4190-886e-1bde948c861b"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6740f054-27f3-4212-8d91-2ecbcd9bf049"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("227796e5-e562-4b83-8def-b52dbbd409b9"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c7fe244c-fbca-4ce5-8243-d17c3cd093a5"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d3bdcc6e-d151-4041-a196-21b18548f6ae"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1a3cf805-9542-42d6-a22e-86998cfc8cb5"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a4f05905-7acd-49d5-8dfb-fbb76c6d7323"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4427e0d4-cc4c-4c47-a02e-f30c302f62ec"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21e8d43d-bfed-4052-bf0b-b5bcd5623cb9"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("75e473b0-09fc-44b9-9e9d-09dde27e157c"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5a6d33a5-7c3a-413e-adef-a693b518839d"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("04f648d1-5c6a-466b-84c3-452c93b5b180"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0cce3fb7-085a-4107-b681-39772df77174"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1f119613-1acf-4a23-b5ab-c88cf976cbb3"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f147a383-3b30-4caa-ba7e-f67d7867356a"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6e388326-f7d6-41f8-8184-e7d85fb07727"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d9ce4ad-cbba-4c2c-9ef0-7075266e5591"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4bd3a069-09f0-4d7c-be7c-c750c3096b50"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("391279e8-029f-4a90-9883-1c5976c88f32"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ef2390cf-9435-4f5f-bf93-34db0f26f4ab"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b951528a-dff6-44fb-9ba1-067ce2910096"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ce4aed6-abb7-4b87-8440-7926f3d41267"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2d8759e5-d842-4a80-ae5d-03a3539e4954"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc140cfe-a28c-4306-992c-413396fd5e1c"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c1eec113-f4d1-4f6d-b8f0-67cf0386bb4d"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca7857eb-3886-488a-a06c-3af3f80199bd"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0e8702ca-34e2-4814-ae51-ba2a944cc1cd"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9fc8feaf-4c2a-4e08-8a48-f29369ebd8b6"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f4f9c6a5-54e5-4836-9f08-116e1caf7772"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd397c66-6d87-4901-9e70-f5dcad0592c4"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("96862b64-c640-4ed0-8e9f-8f48bcdb841a"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7159bb49-17e9-4970-824c-bd82d9be4c8a"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7b892184-75d9-420d-907e-b0abbd0664fe"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1b874c68-b883-49f6-a5ff-c1f161bc4842"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0205c9f7-470c-41a6-868f-8023be923061"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ff3c6b92-a886-479c-b149-bc9af8993b0e"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae179e29-ef40-416e-9a93-a74627779f30"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cc35b2c5-e0e6-4b21-b103-484353451014"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("61a117ee-285b-40a3-a760-779a4b444773"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc0e23fd-47f6-489f-95ff-6282bcd7dc04"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9fb144ad-c1bc-459b-b846-9b6e77c49fbf"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aa613686-dc7f-4d84-a630-18117eec2449"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3d18694c-88c9-462c-bd07-204e2885045a"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("032d7c61-4b69-40ba-a42e-7acafd989b17"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c94c243-eedb-4a86-93ff-e20de905a61f"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("eaed7c6d-b759-451c-949c-5cd73d3e9afb"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("30778874-6d4f-42e2-9a6c-885441fb9ad1"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b1140541-92d9-4606-b872-e84af0b38cf1"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7c5c05ee-6bc6-4d65-af5b-2fdf897b53ba"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c14b54f5-e913-43f1-b516-b801cdb16c5f"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("91d34f4a-feb8-45a1-9e03-2cd2cd613a1b"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("39f880ba-f014-43c0-bead-c14f6192e93f"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3f4a23d9-2f92-4851-946c-89fe0bed5da4"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3dd812cf-8ef6-4203-97c1-cc6f2eb2469b"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ecdff0b-cc32-4ff9-9761-a0d1877963e5"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("19c06911-df30-43f3-a66d-5205fc26bc63"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f40feeec-3772-4ef5-a637-eac5ed1fb85e"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f8f98336-9163-403f-96d9-0496975bece8"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("336972fd-6cec-430b-87b2-a5c24bb047b0"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ce0848fa-3e73-4957-a776-990a804c245e"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5c1a1824-1b57-4ff2-8633-ef59a0381cdf"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0ebd73ae-dd7f-4dff-b2ee-6701c02884c1"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("292b88b5-f341-4f6d-a9ab-c045f36b2287"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e75b787e-ca68-4075-ad83-283b255cf14a"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c1bd7288-8510-45ae-b991-4aa57496dc7e"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85760e5d-e79a-4155-abd0-95beb04610ca"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("57451de5-0e17-478f-b826-b8a9bb4be5e0"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e1e218b-190a-4d94-ad0e-107244de8b07"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("71a99abc-6f6a-4fc2-b71a-c4f3053a1e2d"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a49f0af7-a1ff-43a8-8800-a53720864cce"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("80db78be-7aee-462b-ad38-6a5a470b608f"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c50b5f16-1f41-4708-b095-5a021ca6b0bd"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("08c3920d-a83e-4402-b063-65c2b01ec01e"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("617ce104-40ae-4b16-b1b6-aea45b00079d"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("09a382de-5bb3-43a8-b80f-fee0ff794813"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("35e60eb7-e629-4d80-b337-3cb106e087e1"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e717371a-f27c-4036-8230-1db976e88db6"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c6e0d6d5-4e0f-4839-ac3a-04fee12ab308"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0ae7c0fa-d218-4822-9ca6-c9d207f05eca"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2a877078-fa74-4f19-80f5-2ab8fa50f3bc"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d4a44fda-779e-4727-b4b1-d3a478f28e58"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da3ae30a-a44d-47f4-9588-dbca08be92cd"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8ddddfa0-dd36-47bd-bf34-d44a08a31493"), NodaTime.Instant.FromUnixTimeTicks(17419814249689351L) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserSongs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Users",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserPins",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserArtists",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "UserAlbums",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Songs",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Shares",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Settings",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "RadioStations",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "PlayQues",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Playlists",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Players",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Libraries",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Contributors",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Bookmarks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Artists",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Biography",
                table: "Artists",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ArtistRelation",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Albums",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(62000)",
                oldMaxLength: 62000,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a95a4da7-d3a4-4f78-a771-b9af7d16c189"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5503e1ff-0a2e-4e00-a785-81133463cad3"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ff4b4a5-d535-4ec3-b202-d4ebedbd2811"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3b51b4b4-0898-42f1-9e63-fc73abc8c74b"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c470bbef-80eb-405d-a954-d493cf68956b"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9906ed17-5567-4164-aecb-2fd2f6cbe3dc"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("36517f17-2a14-4b5b-92ba-081e5da74038"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b47223fe-764a-4488-b987-e04b2ada59a3"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("81bdee46-10e4-4c9e-9c9a-845477a8f7d2"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("470124cb-1fd3-4a38-89de-ea36f936d342"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("66bbb866-ab31-43fe-a01c-9e085b9a1453"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae3716a3-6a23-48d6-a669-e3c4fd91bba9"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6a1cd88b-abfb-45ae-923f-8c9b74798142"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0606bb62-f6d5-4872-ac76-bb5df3d886b6"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("84e84fe8-badc-4481-9e23-1998b094ad63"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95967f22-f66b-4be2-89c1-d6b0c747c127"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0ec9927e-7fc6-445d-9c85-f349dd7b16d1"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("973aa487-151b-46d0-8cd6-53f0499509e4"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5cb658b3-09f8-4a53-b50b-90b2efe2d524"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4395bbb6-d665-43eb-81c2-d47aa668907d"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8d91883b-7399-46e8-9d20-95ee4b360212"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a27827f4-66f5-45b1-9d74-d15f01f8640b"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bcb7de79-a15f-4d94-9f75-a8fe5b846147"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("22a991cf-9a19-487e-9468-bc64d1dc1304"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6977d8a8-c5de-4afb-9be6-0ce7e95cd55b"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0e4ea8b8-bb64-4b87-bb99-65c522ce4f07"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ba9721e0-4ba2-4874-a926-57b33a415514"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd5b29e1-1469-4d8d-87d9-afd65b4c1d5e"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d4cc094c-de90-45b5-809a-bad2e8dab228"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8821e3c2-9ed1-454c-bd10-442d2d138695"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5cf4703-4b3d-4b82-843f-cc51fc62f5a1"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b73d91b0-2611-498b-8596-01fc0f0005bf"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bfdd69ab-384e-4638-a6d5-dab0d358cd9b"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("62f7a205-2bab-4c2e-9b1f-9a5132b4ce4c"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e483826-8e4e-41fb-831e-de98a3615363"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f86332ee-d932-4fa5-8639-4471e03d5769"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("469e7313-d3f4-466a-909c-73be17bdc934"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8675e14d-1b61-4337-aa92-92839809037f"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d7988322-f621-4500-88bb-48192050e20d"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5df73f3c-44a0-4035-ba09-306dd279ce25"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da9fbf3b-9b9a-462d-8ce2-f817dd78f8d7"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4caaf87b-12c4-4ee0-a74f-91a699307ce2"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("804f0bec-d3d7-42b2-b8ca-dc7678eeba6e"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a63853d1-f0a6-4f5e-ad62-c854bbb1ff93"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e48028d0-b30a-453f-bf45-bdba47c6e12e"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cb23f5b0-f611-4fe5-9b4f-b537bfffe584"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ea9eef5f-742e-4185-83a0-401ef5f52e6f"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("39ad3bfb-13fb-4f69-b4a8-62b841939f31"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("559806c5-8c25-48c9-a79b-4b3266571a70"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fe4535f9-84d0-4c8b-b1b5-346427bfe834"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("61e1072c-6ddf-4129-8efa-92fcaa69dcfb"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("577dc823-9766-408a-805b-c68f02ee307e"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("abdc60c5-0cd8-4c4e-a9c9-00b9eb2062bb"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5aa522bc-ee0e-4c0c-9b6f-08e10f4bbe86"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("70f44a16-862a-4131-a332-d992f9c6382d"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("736a151a-f9e2-42ab-944b-08f5735d0448"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d9a52403-894e-48ee-ae07-464f8271ca11"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ed7891c9-b3cc-4c78-9fa5-4c64d4387638"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7ea302b1-9ecb-409b-bd8b-5ea1a84bd25e"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4e23de42-7151-46f9-b040-941ce739dccb"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("00a05935-0dce-497a-919a-17cc1f518497"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fde0668c-ff10-434c-8cf7-b013f581a83c"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c779c1e0-6fcb-4b9d-8312-c31c9716e147"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bba60639-fb00-4c68-b591-29d2a41e3590"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e071c5b7-4720-45b0-8ebd-891d39711731"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9f89894e-9201-45e2-910b-655f684e63ee"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("82107526-3086-4397-8df0-0f7ade63db93"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("03c432af-1246-4bb9-a893-e1bf76958851"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f8af6eda-01ff-467d-8ebd-c582d0542511"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ee2bdb3c-28f6-4481-b280-d6ff54d3042c"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95dbd1a9-aee6-4819-9f6b-30ef15b86c38"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("89e8dcf7-78e4-42e1-94c1-956f29a59d2f"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d1ffc276-abb6-45e3-9986-ba1c12227e01"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c3640d92-7e48-47c5-a133-af60f2053d49"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c24fcf63-b7e9-433e-bddc-5b26b24897a6"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7d7bf159-142d-47f2-9514-fba04e77becc"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3f7b4b83-c2b4-4df1-80d0-66b6a3627a3b"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fe42ed33-8b92-43e3-9d31-fe5bfadd41de"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("18e37e0e-2f55-475a-9dba-6a4ff305fa3b"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5d930258-95b6-4d3f-a156-63d3f6a23e02"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("721c5fc5-5a9f-4aa9-9343-7cc316cf0285"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b897a81f-07a3-4bf8-9806-b3652a892558"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("31299759-ed9e-4835-aa84-c2a43a0e47bf"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("effc2885-2f70-443b-86ad-1042f2c1e402"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("270d156c-2f4c-405d-9950-abd46ea5dae2"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a0c1f1d-4d57-43f2-9b2e-f81735d9c41e"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8f708fd2-0e1a-43fe-82cc-c4ec42b53e76"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b70e6248-3b55-4c55-a82a-63855b639027"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aba690e6-f4fe-443d-a5dc-ebc6de1afff9"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dd197568-64c5-48e8-9a93-9562c0f9d7dc"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5cb1bc9a-3e16-4a44-94f6-f1d8bb5213f0"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae60e622-c179-46c8-9dc8-45245ec6e3c6"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3eb8fb0f-d5ab-4c26-a274-3e9d343472b8"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ebdd38c0-5a9b-42f2-bf0a-cc1e47ae1e8c"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0ded023f-db9e-4cd5-b798-ce2c91846bd5"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("34bd876b-6edd-4c3a-9415-66dc1f3086db"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7c7abeba-c891-4a22-a434-f7a9d7f57a2c"), NodaTime.Instant.FromUnixTimeTicks(17408661880505478L) });
        }
    }
}
