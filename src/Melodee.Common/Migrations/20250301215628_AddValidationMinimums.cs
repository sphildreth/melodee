using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddValidationMinimums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1303, new Guid("5cb1bc9a-3e16-4a44-94f6-f1d8bb5213f0"), 13, "Minimum number of songs an album has to have to be considered valid, set to 0 to disable check.", NodaTime.Instant.FromUnixTimeTicks(17408661880505478L), null, false, "validation.minimumSongCount", null, null, 0, null, "3" },
                    { 1304, new Guid("ae60e622-c179-46c8-9dc8-45245ec6e3c6"), 13, "Minimum duration of an album to be considered valid (in minutes), set to 0 to disable check.", NodaTime.Instant.FromUnixTimeTicks(17408661880505478L), null, false, "validation.minimumAlbumDuration", null, null, 0, null, "10" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304);

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("03dc2fbf-ae05-4611-8663-6f98bf23a090"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6b3dd7ca-2a29-44ff-a948-7863d2e988d0"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a3c1680a-f24c-4c4c-bda6-88f6c7f7eb37"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4852c3ff-344d-4df5-b6eb-0cc84b645f0a"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("637f4c10-64aa-4207-b63c-93f3d613b30f"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7da112cf-ccdf-4c7a-9e8a-55154c3bd8a4"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("11be1c54-11ce-4fa9-989b-66c5fb319aa1"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1334f23e-7832-41f6-a937-0e9586acce24"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af201489-0f1c-4b62-a2ba-e2c9bf84b278"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("74cf0f37-1004-41f7-b5c3-a1daf9dd8d04"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("89436ebc-4c8d-4710-bddf-fc7bef8a80df"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("84738b24-e82a-4caa-9907-214aafd5041d"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ff0cea4-5560-409d-b614-9d50e3ca7838"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f4869674-2898-4e2d-97df-fdd38972b8f6"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e1ec13fb-e424-4fd5-aeeb-483b47d3bcb7"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("93ceb337-77b7-44d7-a809-1cdce9dd2602"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a730fdfa-de27-429f-9fcc-ec14ec88c964"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bbcec87a-4d17-4f39-b420-81a16ceba347"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f11be448-1095-4bd3-bcd5-6262589bb77c"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c6b1663-c41d-409b-a0fb-76d501dbddcf"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("58515e47-e18a-4abc-a17d-011762b7f15d"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("65019ef9-d673-4449-b9bf-3bd52f2c1a02"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c3e5664e-bfbe-4dee-a8c8-8af4a3ab9f38"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e29485a0-0901-4d97-a8b6-7698af34932c"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("71590087-df53-4fd8-b3a5-64dbecd4b602"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e721a6b3-879f-4fd4-b847-035201cbd5eb"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6b05d689-573e-4681-86a6-6681bd9c5e0f"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("62c7385d-08c6-481a-84f5-5f97d848c0b0"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f903f962-c562-4a1c-b6ec-2aee6a36f7af"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cc26cb0b-e022-48e0-8b74-b4b3747b2748"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9dbb0096-5dd3-4bef-8641-697dbb313a53"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f4c2e973-eb87-4bc1-933b-ae3347bf6ba4"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("18a790ba-ab6b-4d18-a24f-e3b26a8f1504"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ccb4adbc-b1ea-4ed7-97e7-5bb1df94a711"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7fb531d0-c1a9-42f3-8285-10ec7022d4ca"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cc92ffa7-e93b-46b5-9679-1648d593cc95"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b7ede925-21d4-4741-bbe8-0d121d9077ae"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("55d65535-601b-4c66-a3e9-4aed50014104"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b14275d9-7d01-4fd2-b7d4-9a38e0ba0b39"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8b2f0d37-5cf6-4502-86ea-13ef595c11a4"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c9e98ee1-7992-41c3-8597-25c9574db930"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b1ea880c-6b96-4974-b42c-76d37b0ed1a2"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4afc6747-327a-43dd-9283-8edb3910e9a9"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("40185ca3-8d31-4332-bb04-19e1c5ab5c45"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc4c5df2-1866-4bcd-9c70-5448d1edb0e6"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8e86d6d5-a166-4b07-85a5-4d64d73bc5a8"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7e97fbe4-37c6-423f-bafe-ae26c94c3c78"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cb83f80c-5311-4c30-96fa-5fae64f7c9b9"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("83124393-b11e-42aa-aff3-fbb3b44388ec"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("72bd6be9-3bae-4e8f-a3ae-ee525b8d1a1c"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b236d67f-903f-4727-8a21-270df583bfa8"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e58b1c30-9d06-49cf-b935-658a7c00eae8"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("62c1ca45-f3c2-4e44-a466-7ba93ede2ab5"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1da5de0-19bf-4515-8812-e2dcd1e8a185"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c03dbdc8-e236-4640-8dd8-d364c289e903"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("91162e83-ee3d-4253-8135-f0fbf58adc65"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6fe9b5ba-0ed7-4c24-aa95-3d2bc04868c9"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("837e084f-c4ee-46cf-a32e-25ba5edf24e8"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5f0ab5c-d047-4591-928b-d9c348af9cba"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("94d156cc-8d07-4d9c-83fe-08179bb4ef92"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2c5d8a69-ae94-4981-9830-5af9e20b06a7"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c3de5835-911b-4058-8cfd-d1426c0b83b0"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51c69a58-9ef3-4ef9-8482-efc55862379d"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("584fe843-086b-4094-bf7b-b6f53c5d0cfa"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53f7f9b7-1d17-4a4d-8175-fd32291e7153"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b46b8e22-9934-4cf7-a506-457c6d41008f"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5b1e4c51-8cf0-4af4-8d03-22a9eff5f09d"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("22650ad9-3283-4497-a3ca-0674de40c2fd"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b57dbd79-a1ea-4d2e-a6ca-ce5a611f60a4"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca595da3-fea6-437c-a4c0-30d793010f14"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2fa66a14-96b1-476c-a4ae-348c9f48ee33"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("699542de-51a1-45bf-8551-65a5223f9e29"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fea15dc3-eaca-44a8-80a6-c2d8d3588de1"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("64fb3072-e202-43e3-9420-e78a9931a211"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aba9106c-f67d-4b93-adbb-cd307bc2d47b"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ecb5704-ef1f-467c-bc40-3d662464f260"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85dd6de9-74fc-4145-964d-7217a4cd4100"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2db35c95-6251-457f-b37b-68beccd58556"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bcf9d024-a28c-4f85-8413-e3a3ec53f26f"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7f3bdcdd-358e-41e2-acf0-a5a851832b11"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b58e8108-c139-4ac6-9374-bfd3285c283f"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("62b373be-16bb-474f-b8d3-c5a26040333c"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("82b831b0-3ee6-46c6-933b-014904b5f762"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f2472c26-5e61-45e2-8ee7-e90a12715603"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a35bd21-76d8-498d-909c-28b902b9a2d6"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("38c7f2b0-96e5-4895-8db4-5e41a403637f"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b86cf297-01eb-4f32-ba2b-2fb571676753"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2125fbfd-300f-423e-b966-e3cfc7f76f11"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("efc590e1-9bc5-4115-a4d7-21e3e131ee3e"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d76d78c4-a64f-4ea1-8473-7950efcded58"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("64b0e3fa-b37a-4cd7-a29a-781e38c88af4"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("078b3893-5fe8-4dd8-9d9d-04306e6a5980"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c6fc1aec-e968-46bd-8ae7-3826536bb31f"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25ef5d73-aa44-43c3-b28f-6af1054a7c87"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2fed5728-37b5-4f33-a341-865e744dbda6"), NodaTime.Instant.FromUnixTimeTicks(17405716300586560L) });
        }
    }
}
