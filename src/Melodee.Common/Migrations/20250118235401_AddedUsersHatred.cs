using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddedUsersHatred : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHated",
                table: "UserSongs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HatedGenres",
                table: "Users",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHated",
                table: "UserArtists",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHated",
                table: "UserAlbums",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cf6d1858-72dc-4439-a519-1cb659029ba6"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95f5fc7a-8631-42fc-b479-485d92fccdd1"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cb5a4fd2-a403-43e2-8623-51d4755f1be9"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b552b4e-c6ab-4229-91dd-45bd1d68a564"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8d8600c9-026c-4ead-b8f8-3a16098a75fb"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ad42a06e-2f72-4f65-99ba-249fed15fe48"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("932b04e5-b74b-4b98-8fd2-19fe95d0eaa2"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("eba29a54-1fc2-4cb4-ac25-d090cdbadbf0"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("92e5af63-d3a2-4fc9-ba70-d4fa125fd6a0"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1be5c088-6570-465f-be36-dab994271362"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0af24ab5-b219-41f2-a4d0-1f456da3e573"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aed80a52-6d2d-4f51-b909-bee549a38c2a"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6ccc5c8c-ce3f-4158-8f82-25ee9c07a583"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4be4e91e-b3bc-4ebc-9ef8-24830e5c494c"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bd036971-c6da-41fe-8cc6-63f231b195f3"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d1455c1d-c846-4171-81b4-6e891c649240"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("466b26ce-a8ab-4946-9f03-14431b181474"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a02d54bd-c0e4-4a94-b881-2e776dcc8a63"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("09638c5f-cdcf-450e-86e9-221cac7b4a65"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd2cc163-51a6-4591-9a99-aa6bb20b9320"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("224e6d9a-4fdd-4c5f-b62e-51c81b645947"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fdb5fc1e-fac1-4114-a4b4-7b5f936f779d"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2845d048-461f-4a92-8439-2794c8e9f493"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bfcf49de-0321-4b75-9dfa-79c53221a90c"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ebf564c-cdba-40b4-ae9c-692ca8ac3a55"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5babe33-24c3-4aa6-8d46-8f463519e49d"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("917cff23-e55f-43b6-9aa7-4d7c64c53b6c"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("72308423-348a-4def-a66b-03741b9d0521"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a579a0af-fa4b-4a51-a553-fbdbb00f7f6e"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fe56f1ee-5299-47f0-ad1f-9f87dbf28a07"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d6735448-715a-43c9-865f-0bf436f8a736"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c4ffc6b1-d1e1-4660-bb5c-175a9dabb7aa"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21e6d577-70eb-47e0-8612-a7869beaa24b"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("07759aa4-0607-40bb-9920-c5efabb0c075"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7a498621-35e6-486c-b544-1f7f1f192a07"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f29b3b18-5f36-4444-9304-f2dac25f482c"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a0e0d6db-f4a0-4a17-ae9c-36a5184da6a1"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("99bb4346-222c-4473-a761-0f056a3682d5"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4da7f164-3666-4755-be16-054c2d35ffbb"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("962856e5-b407-4824-9b14-55b6b21847bd"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b6913585-c084-4219-8a04-94464a172423"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0bf56bfb-d032-4146-894e-5df135d96a0c"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bbbb9e2a-d45f-4bb1-9cf3-6a438397081d"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1721ecf9-ab04-4843-872e-7f2a5078d5bc"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd72abff-bfb4-4262-85ee-73784667f92a"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bcd26767-7818-4af5-a64d-000806bf6ac5"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bd894fd5-cdec-49a2-ae13-a81eab591473"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cdeba3fd-47d3-4714-894f-e9d664cb3aae"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6e7ddd89-e1b8-41ef-a8b9-bed8fab9f8c4"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e2521979-22f3-4472-9f10-94a1d2a610cf"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("790533e0-0f3b-4ff9-9405-04cf42566bb7"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e627546a-f594-4eb9-8573-607a3745713f"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("707e4c92-96aa-40cd-be9f-4d041481945b"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("634146a7-9960-44e1-8df4-3c78ff170b8c"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("751fd87c-cc5f-4b11-8721-678803ec2c3d"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c17fc19-efd7-46d7-bd5b-0b8aec05cab1"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("88de8a6e-5da3-4007-a769-9175471c5706"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("22c11167-92e4-42a6-8671-488a1be81d6b"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fdedcb2a-ab0a-4a58-ba32-19f5529cc829"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f03439bd-0a1b-4905-944a-0101cc729762"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5883a5c7-4824-4c64-bd3d-ee0aa803f7f0"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0f181317-c3c2-4f1b-9844-33fb8036bcb4"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9f1ff739-d316-4429-a171-80e914c65e56"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b39613bd-395f-42c5-af22-1838a23ed21a"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("44dd6c95-0837-4b14-9e5c-57e1dfe05a99"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e59b7a49-6b3d-4e0d-a106-428b2722ed85"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d74f8f4e-8d6d-4ceb-89f1-02fc79072687"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3625fce4-874b-4619-a6ff-a73166781435"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54df50a8-863e-435b-bb34-04a392b86ff1"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("907a82ec-ef4d-4e09-b263-a4d9d1403ad5"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e8044f8c-2942-45c7-a82a-348dcb287f76"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af152918-cf0c-45c8-93d6-1c47e849b4e2"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a92d54a0-62d2-4e63-b4fe-1b23e8ee67e2"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("274779a8-6978-4cbe-a8b7-747a5e64efbc"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("06389e1c-4500-42e7-ab8b-2258859a4e67"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dc1edf32-963e-4a4f-854b-96162b96e417"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c05f07cd-a29f-4a71-9119-dd59e703d235"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ccd3095-9055-4545-9eb7-0405b1f6e1d0"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("846e17e9-062a-404c-834d-52d1c69d2e0f"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("20ce6e79-cc4b-4e51-bfbe-5879ba61fa59"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dfd48f3f-c8d3-43db-9734-53d2db7fa617"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("38469ec4-23f7-4032-9724-429b008de93e"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("05d00ca4-c2cb-4884-8b63-35c9c0dafb61"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("78b781a6-7d9f-4716-acfd-a437c1df98e4"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d6bbcfb1-a629-497c-9b1d-1a44d11701e3"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5b7ad24c-11f3-4b8f-ad78-9b4efb46b09b"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("64956d06-41e4-4ec8-8ec1-e191a6138c30"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a0ec6c2c-cb16-4e0b-99ce-ebebca29cfac"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("869a45cb-50bb-49d9-abfa-fc2b89b2a0c6"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("20c69eaf-5e55-4d19-a771-eb7768136dac"), NodaTime.Instant.FromUnixTimeTicks(17372444409814740L) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHated",
                table: "UserSongs");

            migrationBuilder.DropColumn(
                name: "HatedGenres",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsHated",
                table: "UserArtists");

            migrationBuilder.DropColumn(
                name: "IsHated",
                table: "UserAlbums");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5f953cbc-4ef3-47f7-9c28-49324bac364d"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b019cd5d-cd02-4cd1-812a-da52a0b8176c"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2ed9133a-3dca-47cc-8ded-45a721a70825"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f2c3a7e3-b44f-4c5f-9ad2-e2ef674ad45f"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aa167d3f-6c11-47f9-82b7-862ac0d038c6"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cb0f01c2-5faa-4c64-8027-ee930e73c631"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7e6587e2-b32a-4c4f-9ef3-4166a07e55ba"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d19daa62-783b-4e90-bb17-6e45f723f92c"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8ec1af39-6d5e-49fc-a53c-a2a11aa53f6e"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f1951cfb-70f0-4ef5-b363-c642bbb1712c"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("07ff0ded-bbae-4413-8bb0-8179359f8d45"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c42237e-e19a-4165-81fb-0b9ac4801260"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("304beb20-5d33-40e1-9059-2a6c362f8e04"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("93abdc66-3c9f-4f3d-bfd7-fcab278e86e0"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4cf6a904-934a-41ab-a777-39ebd8de69d1"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dbb37e6b-dd4f-4f8e-8988-c7c40d5c9603"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("01bdec1e-1302-4781-a534-a5bcedc7ca15"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c610165c-0215-421c-b0a5-a9ab8a576057"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("36dcf5f7-dfd2-4ccd-846b-bb8130c803c7"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("372d9c56-b797-49dc-a07a-f676baf29eb5"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a7722775-2ee6-418b-a0e4-a7227465a0d5"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("97af4e3c-9597-4b35-ab49-febe64a48227"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("34f939ff-da35-4f87-b676-8216f415fff1"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a51c5a23-6182-4bca-afc1-0b78a208a1f8"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2257d212-1406-48d3-a449-e82c800956a8"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("543dd0c8-346b-4c52-8417-94d3ce493533"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c4e01e0e-a0e3-4e69-88ce-a2fbf4a67f34"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("301ae816-be45-40b2-bef1-e7d1539c1e04"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("216d8363-3bc7-4ed0-a6e6-ca6fee74566e"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d7ad3d95-47b1-478f-82c4-7bc22af4f8ac"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d860c90-0dcb-4a0a-8ce3-6611f24b02f1"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1e311148-4f34-4b95-a02e-e9384cd8bf24"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f41c7cc5-83ee-428b-bb93-7beaca456455"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e14d6a85-ab70-4989-bc20-f0f3541968dc"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4a584b4c-5647-4a07-b187-a7b2169674fc"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("18b95caf-9ba4-4cff-ac3e-094d6bb13aa2"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e8249d27-1bc4-4ee6-a21f-eca91b7aefde"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e85f312b-939e-47e6-a607-88a27c319ea7"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ccc5f7c8-5e88-4cca-8761-cd77d9fef388"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c126ea27-c92a-4446-ab33-cf3a5c170a9c"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("188fb880-3df2-4f91-bc31-602e4d92b64f"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2cf94625-4324-41fc-ad5a-af7b6d6ac6d4"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6071b6de-2750-4540-b7bf-713034e5e5ae"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85e35269-cd74-491b-9282-15a2bfb5187a"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("48e9a8c5-248b-4749-b170-d88be4bc5abf"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7580316e-7425-4027-8a31-6f693847e2c7"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("56ec5781-579b-4981-bd40-1ab132269fd3"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("81f76073-5053-41cd-9e41-d833e2c12f0b"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dd6b2f74-7aec-4dce-927c-9cac5d09a68f"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("68e746d2-0b51-4c47-bd85-0c52a026ced6"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5e2affe3-3a9e-44f0-a4a9-2306dfee31a5"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("99648913-cf45-4eba-8370-ed01628d0373"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a39deadd-e980-4bb0-8078-92a35d24a37b"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d1ebb3d2-a843-4c29-b9d5-b4c8f98722c9"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("eed08ca3-6ea9-4a3a-94be-707ba208dc97"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f263660a-6b0d-42ed-90d9-38a362401466"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2c6df212-6be7-4f45-877c-d7be4be10feb"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ca8ba25-b2f0-4df1-adb0-c3ddcdc9cbbd"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fcdf6b51-58e2-4e67-8e09-78ef021fee3e"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ebe7b100-5066-40b1-ac80-71f7e80b040c"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aa8c01ca-5a58-4e8b-8cb9-c3b8819c405e"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("436639d1-f5e3-424a-9368-f0f485ee7985"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fb6f0842-96ce-4a7b-b6ae-313c8bee2152"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5ec30457-4f76-443d-b947-a74f11b5e113"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1b81e8c5-ae7a-4931-a783-47480dd6257d"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("96aa31a7-6cb7-4e17-bacd-ad5167996473"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ab6f32ab-8aa1-431c-a75e-4c7d98e6d834"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51ecd014-82d3-444d-969c-9e28ce892c65"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6950b2cd-a104-4787-9fb7-30861a75afb5"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d4b022b2-6bee-4703-9f6d-589a3a107dc8"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3236dd59-0777-4451-b16f-943a93d3f4aa"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6d05dc04-9914-4f53-a51a-6b4a34060e47"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cd0dea11-54da-46bf-8c34-cb6bdd90aa1e"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b0bfc226-f67f-49a9-a996-b2af50471554"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6adbf406-f035-4b5e-90f1-ff0fb4cf4095"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e897cefb-c0ae-42ae-825c-582bae99ada2"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f28bf870-5f06-42dc-83d2-21dd77d08bda"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("01fd7eed-f8af-4c68-b9ea-e1bdc6f1f602"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bf9b60b6-8e64-4178-a349-858abb3d095e"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("780c9d50-996d-4380-a045-01708d98ed15"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da9604e3-893e-4205-840d-75ce99fec52d"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7185793b-9981-4655-88af-3d74abebabf2"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc852f22-8b53-4cd7-b86e-168d9db53977"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5a6121cb-4c5f-450f-bb57-db4629b8a6a9"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("32234244-519d-4289-99e4-d109d7321f99"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ac4bda0-7ea7-4e36-94b1-72db270c684d"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("01bffeee-a5ea-43f9-b139-68ecbf1b09fa"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d36f45f0-0e56-43be-8de0-98b25963ec96"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5e7d101-c2bb-40b4-a075-914a0e0ee37b"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2ad41e3c-db7f-4041-92b5-256e58c2719e"), NodaTime.Instant.FromUnixTimeTicks(17372195448732606L) });
        }
    }
}
