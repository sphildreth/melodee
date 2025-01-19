using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddedLastFmSearchEngineConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("72ee2434-983f-41df-9e5b-035f692fffcc"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("70da6d8f-1455-4f78-9eca-1c4156795825"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6b04a460-37e3-4a4a-9a50-bdc5c33ee6ff"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("03ba163a-ae0b-418f-9a55-2cddc8508a16"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca608c6c-dd72-4e01-956a-f0d24b0c37d8"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9aa7baad-5f58-4b89-b4a6-d04aba786052"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4bb3abf8-9cf4-4b2b-9dfa-91167d01a84e"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e392f153-0e3d-4c92-b569-926fd3eaab22"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cff4c258-517d-4605-a956-fc33cabdec56"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ac45500-eed9-41a6-9b21-aa53eb47101d"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e9c61c23-67a1-40c2-a1e7-66a6e33b1728"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6d240c9a-df96-4aa1-b613-4bb671a38874"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b6414573-c6be-4d32-9531-7e1d98826c48"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("69ee4b22-1d63-4dc8-a2ad-0396750481b6"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f0c5bb1e-5b25-442b-b0ca-bd7a9b6ab696"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("13d2785c-095e-40a4-9020-7570db08baee"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a4e71a79-8354-4f52-9c53-3036bb50c209"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1a1cbe0-8ab8-4f5f-8766-2476faf76b73"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("afc26cd6-f6e6-4e6d-b956-cae416fa157e"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f697fed9-bba2-415f-af28-3d60537d7e67"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7b34d7e5-ed83-4fd3-8631-27be781760b4"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("00912a1c-90a4-4349-8318-2aed8dc7fc8a"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8a03909f-8be2-4b55-8a92-38c3746fc807"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("664cd17a-e6c0-471f-9c7a-3e4f1626bee0"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aad7c82d-e883-4f5f-a053-bb777241e8bc"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d611f736-1868-4f51-880e-d2b807ac5fb7"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51c9cf8f-20f1-4cfa-b845-68ef81a1d112"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2cb5291f-ea21-4819-800c-e614b2d9bafe"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("40c48b9d-5cff-4d0a-a2eb-ab278abb8921"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c574b5cf-b48b-424d-941e-5c2a678e2abe"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("24e6f0e5-531f-4d34-aefb-0104ee044e61"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b7f4d4f9-56ee-4555-9da4-106f0f81f489"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bdbad89a-b1fc-4449-a47a-9ba6d793bc22"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("364248e3-d9a3-4e5d-b4b8-a5af02786e7e"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("61d00980-22ef-49e1-b751-96dc66032fcd"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("20684744-eb2c-4184-8a9d-42f10db3770a"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8f1217c7-1285-4c9e-87c7-da389aff86e7"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1abe4c1e-eaa9-478d-93df-c3d0b4ec958f"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ccae04e-a9e1-49d9-ac68-18f399557f97"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5657e5e-e59e-4e8d-aacc-a4b8f8a50db4"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5f0e06b-a61d-476c-8951-45e21ae6f9b4"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("50bfe543-f521-410f-bd5c-479ef581e177"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("561931aa-359e-456e-9164-8d2ac060d402"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0c2c6477-a626-44ff-84d5-2c05b8f06b65"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("23e5634c-2f2a-478b-81df-613a96df16e2"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0c95d6ef-6804-4781-a886-b6a164b36f11"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9c6aa5c1-3e19-4c1a-a59b-f4fe531716b2"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("11a34058-eb4a-4ec4-af79-29cacadf0dad"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2c495cd3-0789-4cfa-ab00-572d0e19e9b4"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6dfc9ecc-4e2f-4aa9-ad75-3107bf66a8a1"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("74a6703e-2a16-470c-b1b4-e73ffa10857b"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6204e41b-4038-4045-8e98-ff22e950ee53"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d80cc5f9-b469-4390-821d-025856b7d359"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6db0db66-0ef4-41d3-ad11-a1d7d8847165"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b918c69b-6968-456c-b489-cb9864d7ebe2"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("364178bc-c939-437b-8c13-e0797939d4be"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3979fa32-7d66-4017-95a6-be1d13e49244"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c947eba7-426c-4011-b35a-fa7c45aa0f54"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7a249221-803a-466d-9440-54303761c060"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7d550462-2710-456b-9962-3de3443ca34d"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9f4d3f02-d71e-4434-a726-2d5b690cd70f"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e0b63cad-4be5-4498-9e9f-8605cf92774a"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ab35c7e-4567-4ebe-86e2-bd67f01568ba"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51c94d68-1cdd-4d90-b9f3-11783e61db9a"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("42760472-29c8-436b-808c-b977f0bd8540"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("935fcfc7-3694-4bc5-96ec-12c424ac2e33"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("82b3929d-a601-457b-b117-fcba385a6fe5"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e4aac37e-6248-4e50-b2e4-82bb0697ecfa"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("280bc799-f777-4acb-a344-65664920d79a"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d0a97916-96f6-4181-a407-575bba43f1a5"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3927722a-9450-4dd9-af33-0b0530c8564b"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a4e58960-5932-4be5-949c-d51c49b204ed"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da616484-4fb8-449e-acb7-c7cf8fdc676e"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d9a72a0b-99f1-46a3-bfa5-88f67ed6c0ee"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("616ddce0-561b-42df-af07-dfffc6c387b4"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("de16d13c-e12e-44c3-b061-f617b4801b98"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5dbce061-ed29-4655-bb0d-8e5e787c832b"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c19e06b0-40c8-472e-a2ac-5d41983337af"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bcd21df0-4dc6-4198-adea-968d9c128a15"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("91b5b00e-581c-40cb-ae27-6ac2d09f8bd1"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a46f0ebc-8067-4d6f-850d-fa8366ab9c7e"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b34a00c3-09f1-4e97-b1a2-617d6f935f62"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7bb9231c-3b23-4cbf-bc3f-78d7e6cfd431"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("571c52cd-a459-4f99-a53e-905be1a44ce8"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c00798d9-0f33-486b-ba4b-54eaaffac055"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3aa9c8bf-7ae3-4205-bc35-92f91d79fbdf"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1810c075-ca2a-44a0-bb99-9afd00501468"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b96d6309-737e-495c-a269-ed6910b206fd"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8b6610e0-0ceb-4188-9f99-f8916911b1af"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c503590d-a0fd-4094-85d1-08791347ac13"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 915, new Guid("7f3e6617-cc9a-4b6e-9cb3-ccc5d9bbc04d"), 9, "Is LastFM search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17373080253925050L), null, false, "searchEngine.lastFm.Enabled", null, null, 0, null, "true" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915);

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
    }
}
