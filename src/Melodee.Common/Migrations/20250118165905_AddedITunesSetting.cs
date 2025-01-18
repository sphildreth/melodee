using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddedITunesSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 914, new Guid("f28bf870-5f06-42dc-83d2-21dd77d08bda"), 9, "Is ITunes search engine enabled.", NodaTime.Instant.FromUnixTimeTicks(17372195448732606L), null, false, "searchEngine.itunes.enabled", null, null, 0, null, "true" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914);

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("964bcaa8-3b6e-42bb-82a1-7fb13e59fd19"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("18a79720-a268-432a-8213-3079576648e9"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5945301e-476b-4b18-b3b1-a33539ee302c"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b1f54632-c8ce-4c35-9fae-bead6e9081e5"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54142f96-3220-4045-9215-274d104f5079"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f8ab7839-18cc-4e67-a4a1-abadeb3bcd7d"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("04ec139b-b062-4157-b107-6a11fd18dcf0"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d05f100f-9fba-4ddb-924f-17fe8f00bfe8"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("69680881-9fc8-4149-89e2-93204c5140d3"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("48fa7381-e8ee-422c-afc8-0ed60182d2c8"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4d3136e1-90c0-481e-8950-4d4659831c38"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b3cf14ad-40db-4175-9aec-4de356baf9d4"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3dee1a61-d967-4d81-b2a0-b573fdaac321"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b3d0c6a3-fde3-4fc8-a2e9-2aa758747921"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b866fa6f-d2c4-412e-b93a-e6ffadf794da"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("05bbe20c-fb6c-425f-a28d-5e3e8d812354"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e7668ade-67b7-4942-bb20-9104d11c61af"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("46a8e326-3262-47a7-aa6e-4069df551572"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b0be02e2-f92a-4d8a-8615-ea0003424ca4"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0123e02-d165-4136-9eba-1b20b821af00"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9f3ad059-435e-4d29-ade0-b09ad2e29949"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c39c7a1f-876a-4a95-8aec-178ffb5566e9"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0785626a-3dc1-4817-b55b-789bbc0ee3a1"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f0fb1cd2-6731-4ac1-8ac5-b862a168a194"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c4a9df06-50bb-4b80-84f1-8ee9a4bd1d8d"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ff60c25-fb41-4af7-aac0-7c9a1f11b315"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3086eb51-d0fe-477e-a3f2-9af1e48fde91"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("571935f2-2f6e-4e9c-a5e0-f820ffd6a100"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("876c274b-01c6-45f8-9c21-0562b0b14cfe"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c515d2fc-0a2c-48c3-a69c-da60048bb6a2"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cd4e46a1-a54b-4b7d-83b3-e92063358cf0"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("083eb519-c5c9-49b3-b456-601d52c2bd52"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b355923e-5ac1-4a9d-a2c1-82656f635460"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("43481a06-8146-418f-9655-39e858e29ba5"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d3e4a7fd-c7ed-4546-83bd-d5553c35271f"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("de801d2d-8ab3-4986-9e5a-5e524eb598ec"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("44705249-4e48-4ba8-940a-871abc50656d"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("10c88275-3ea3-478a-81f2-d40263d29695"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9f6478d4-3045-451f-85dd-48e3651003af"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("40c778b6-fdaa-4809-9884-020ad6fd8d14"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5246fb6-b679-40c8-8e31-77e53728a415"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f7ed0d26-6107-4f43-9805-ff7ef34fe85f"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0f2bff2b-7100-43a3-8cef-284203e758b6"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("479030f8-6b09-4bcb-aeb6-089c644e542c"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("806c8c71-d0e9-4658-9e0c-7940daac88ff"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("398bf2ff-9ea7-42cc-969b-d8b9848372dc"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6b24a491-cb61-4b12-8a50-94c3572f6cb9"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ea8926c6-7c16-4b39-b895-6be61ddc8f46"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f2c0a7ba-749e-432f-abc6-de3076162fbc"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7484842f-6f88-4611-818f-192296fe3e5d"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4a22f6f0-effe-4fdd-bf32-433f7146a128"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8e34740d-2ffd-4d82-b89b-c62ab47a1a59"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("75f16c0e-f92e-4259-b72a-580d7bb9aa33"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1978f799-f00e-4f3a-ad44-e806b7b2c7a5"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b0617195-1f2f-4415-9581-c1126e20d802"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("01e7c000-3ec3-44f9-ad4b-614b6067f488"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a9ebac4-71bf-4e14-a4f1-6422c38abaa5"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8086c0e1-cd0d-4875-9cc7-79b2a3249e62"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("569be649-5948-46e2-aa21-057d948d078d"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a27d13ef-81a8-4914-afaf-c2b511c5f961"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("897b80ef-9dd8-490d-a902-c3649131f152"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a521035b-5884-4787-9fc8-53fb13ed3073"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("71108cdf-04e2-45a2-bf0e-cc1b536f1569"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d237cf86-7536-40a2-a86e-b9bb1344ccec"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("718f634c-2b9f-4132-8418-5aa212bc38c0"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("107497f1-a2f1-4dbf-baa7-f86b456387a5"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fb1c7aaa-a62c-4e1f-9043-c5760715f7bd"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2906c705-cb18-48bc-ae28-7b4e87a3cb94"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f87ab745-19ab-4fc0-b042-e40e07f1b7b7"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5a4b2fa0-f328-4791-9c86-acd5e39d230d"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("812ed753-76a1-4ca3-8d1f-dc1e03a53df6"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5e8e8a9b-ed44-47b1-b17f-4ca8a938c63e"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2fc83ae6-abce-4370-ae74-481bd987e2c7"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cf5aa8c7-4f9e-41af-9ab0-4a0f052f3e5f"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("57973033-29d7-4694-b536-bf456ee41ac6"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("751c53ef-4723-481a-8719-fd17d8fe50ab"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8e0b441e-c21b-4b90-b718-3728b743eee3"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4066d602-a8fb-4716-a9b0-ffb371411822"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("758a8b0f-e104-40fe-a4cf-841272a20472"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bb477b03-57f6-4aa0-8cd8-b5438332dc56"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3c014b15-35d1-4830-b2db-c5f5c86a3741"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("97b3d060-3c9f-49f0-ad76-5554c88973ca"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8d920d0f-2570-41fe-9490-15de75c76814"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c66c15ea-1084-432a-a0ff-33a88448c444"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("351c01c7-95fb-4274-a8e5-944a44dfb185"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b3078031-53a1-43bc-93db-6a70b3f4371c"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2d3700c1-cabc-4f56-91be-d4312bbc8eb1"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0773175d-638d-49e8-9e1a-41c75bfc9d69"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6a55d4df-1e19-4dba-8faa-b8aa402a509b"), NodaTime.Instant.FromUnixTimeTicks(17367206387499076L) });
        }
    }
}
