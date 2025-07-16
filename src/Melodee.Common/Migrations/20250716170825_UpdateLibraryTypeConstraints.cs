using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLibraryTypeConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_Type",
                table: "Libraries");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c812ffa0-1cec-4284-aa1e-71a46527758f"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a566bec-f5bf-40a6-afae-c0ae342e9c52"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1dd63cd8-1f62-441b-bfde-58f89559ac69"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca1b5183-2816-45a1-9c6f-e00c1727dcee"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8464bc2f-47eb-49c8-8e8d-39ff88c84a5a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("75db42e1-f0fd-4295-a7db-6104656dd57c"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("71c2bd5b-3069-4244-a313-790f739646b7"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9287aaa6-0982-4fdf-8f68-7dc64c514abb"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("80ced0bd-8ee1-417e-bbb7-a55b1699fb05"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a0547b74-6be9-48ca-8a33-dcb91361c25a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("28feb17d-05bb-4d77-9298-2de9b6d91846"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dc679c03-d4dd-4ea2-88eb-9ad2590ba414"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c79a6018-2b7c-43b7-8c41-3bad1b9224ff"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("271c276c-83cb-4396-99a2-2a5570b90170"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("099158a0-81d8-4113-87d9-4f730e051a1a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2fb29065-264a-4275-b6ca-9a40fb876759"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54836d03-cf47-4617-a9a7-41d354413346"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4422b5a6-0790-4ba7-85e3-02e5db8984bf"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c1f8f96a-6d8e-4b15-aa5f-c611495f4bc9"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("00443259-8179-4695-aa3d-e9f2f411c8b8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8f8a67fd-9056-46a2-bb6b-2d4f02318c10"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c63530a4-35c6-4562-a2e9-8bd961d781ea"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af674536-6a1f-480d-a7c2-7774546afead"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("897facc1-8657-4c7a-b927-ac6dc8d9ab86"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e209426b-d25b-445d-ab33-7e47e497fbdd"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8fbb5a76-10b4-4ce4-8886-4246336bdb2a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fda34b2c-1ce5-405f-ab96-4750ad99ac44"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("668ff787-7b67-40e4-9bdd-03ecba8899d8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b009dd7c-4953-4919-9632-b3bac6cc1ff9"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e6e704a-bd87-4b52-90af-07258be0e04d"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("52bced75-d3df-43d3-bcad-612368fa018c"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f5c2326a-d11e-4f47-9980-bb64859fd7d9"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("87c28758-9bab-43cd-87e0-71f923c16a44"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("053475d4-5238-4805-9857-4c8c0ed1153a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt", "Value" },
                values: new object[] { new Guid("12e25983-cea4-48a7-9f7e-9a8891dcba97"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L), "1.0.1" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dd177a42-6c2e-4885-9841-82644256651a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25dcaf5b-e0b9-44e0-85f7-957da7798954"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1e87aa59-4703-4a8f-94a4-d8b7c874a060"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("91a66eca-2aea-4837-8b8e-87cda9594ca4"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1fb3000-d26f-4872-b2a2-9654f0b85150"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0f0155b-ac6e-40ff-b8fd-e22d0d2a69ca"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25503f74-8750-4445-a023-971485537085"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bac12225-30ee-44a6-855b-c237314fa2e8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d8999f67-cbee-46cc-9fda-2b3f049a8386"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ae77ba5-8564-4585-8611-d23d00888e60"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("90a55fa0-e340-42c1-bc2d-a84e8586739a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8aead14e-f328-4d30-9c8f-40109f2957a7"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("29f1118f-1d08-44df-a72d-f8fde49d45f5"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8e380792-56d8-4355-90d1-49509d6491ff"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("755ad2a7-f096-4993-8ce7-de98a0d44490"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("482b180e-811d-44d0-bce7-8fb2d581d9dd"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("04b1127e-a11b-48a9-a346-75a1cc694cb2"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("358026a5-6c9d-46a3-b7be-27b96f62fe69"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ca2b3b2-7a27-46fe-8c6f-aee488aeea1f"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4b422f00-21f6-4369-82ad-001c7c58adba"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b3b81fd0-42bb-41c3-af63-bdaf8341b7f2"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("272d2704-9dcd-49b6-8f25-cd72f686d2a5"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7e3c8ba3-0832-4dbe-b029-6d91273dba60"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1a00773d-404e-4af9-ac46-f8b4bb56e1f3"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d3f8f971-83b8-475b-b96b-73ad397d2a91"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9a1f85f8-f606-4aac-8c55-8033f484916e"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("77cec88c-4dcc-4b25-8eeb-ae9c29abed25"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4725dd10-29fc-4e5d-b38d-38f8946c1445"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f98dcb5c-4529-4d24-b360-fe9c9c495b02"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1206d37-92e5-483e-bd23-30d4b30f6951"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1f07524d-9abf-433d-a0ed-33e089ce2791"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fa52d9c3-c7f5-450a-bab0-5d78222df447"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bb1072b7-7cc9-4259-aba4-136961820fa4"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0bf4405a-6b46-4da1-b730-57abacf02ce2"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca61ffc9-5f89-4ca1-9031-295bb987ad47"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("add6f03b-951f-45ff-bde6-8488bfa58bc7"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d73c1333-adc1-47a1-9141-dccf13ba1a6a"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("22bc4a0b-0967-4847-a073-b1c2ef9d3c9c"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a8465b7-4ecb-4c34-b6a8-9de0dc0a73c5"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc5c1c6e-1808-4ea9-8431-56708457d26b"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d7f8a03-2d57-49fe-873d-8e3126da2c27"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f360d8c2-e52f-4d44-8050-5b9b9cf0bb9e"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21546512-4fb6-43fd-91ad-dd7b342d9922"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("301d5e01-b81f-492e-90c0-125a4c58ac10"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("70e4e2d8-40dc-4dec-84e9-ed3b9f774fd8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ab9d2a8c-3abb-460f-810e-3ace5ef5bf36"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0f75e577-cd92-4bc3-b09a-1a2248d259c8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3df92484-5cc9-4a90-a70f-6244714e7286"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c83e8344-bd21-472d-8e19-153e810455fd"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("30dd9542-a329-4082-b81e-ae625c279ece"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3388b42f-51b3-4f6c-9ef8-8d73f1c83da0"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ef96876-59d7-4bfd-8e11-0c94ce25b0c4"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bb1bb972-df84-4b51-b30f-6c0e84141bc7"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d2e2836e-e1fb-400b-a64e-f2d5c45c9cbb"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("015dcbc0-719e-44a3-80d9-49491352c659"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2af5baf4-87cf-4bc6-ad75-d502b7ba4ee2"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("45fdb8eb-e4b3-41e1-9caa-7eb23a2cc28d"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("547807c9-b4c3-461e-9c8c-e426f2c3f096"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("004e4d01-d163-468f-9e2b-67198f6e9291"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("10ce7a47-0059-4c9d-bbde-3a5c725289c9"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aad1ee27-017b-40cc-84db-4708869f9d87"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("93ee28f7-e23f-4cc1-a10f-ee48dd1fa1a4"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("13a18711-f791-4773-acfc-03fcaeffd4c8"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3dffe787-4e27-4215-bccb-18dde5f6624e"), NodaTime.Instant.FromUnixTimeTicks(17526857043438029L) });

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_Type",
                table: "Libraries",
                column: "Type",
                unique: true,
                filter: "\"Type\" != 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Libraries_Type",
                table: "Libraries");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("44175952-2a41-470e-b240-daa7fe6e1ee4"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("38270c51-5757-43a2-b422-cd8f92c10e27"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("65c4431f-fd58-4025-8bd7-0bb9b2f63594"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dc3bb2ea-d4d3-4f87-81b1-d12b19a0a0dc"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("35f4ddec-6b04-48cd-97ee-01eaad888c13"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("64c097c9-09cb-485a-8624-a6d11dc4855d"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ebb88fa0-ceca-4a37-b4a1-a46b1b8a8b47"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ce6340c-7ae1-46cc-818d-41838a962923"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("17cbcde8-960e-49ba-86c5-c75af2a48f1d"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("995ad789-9769-4c24-9d26-c389886febc1"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("64cf87a0-73b9-498a-85c5-529f1aaca254"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a08c0cc0-9fbd-495e-91f7-8328462abfb2"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f9d0a19e-0f13-411a-b11a-c52903fef855"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5e09a8b3-c3d1-4131-8e9d-5f9edd167442"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e3b88163-ac5e-45ec-b78c-2240acb0e6db"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("275ed7d5-469f-4b0a-a941-63dd2ea31a56"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7c4afe94-27e3-426a-ab2b-84e75bd6e017"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ffb01250-fe1d-4782-b3af-47df1722ae65"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d37a7af-0055-472d-abc5-9fcbb3529faf"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f2117f0c-28bc-4f04-ad3f-134dfd0f2e2f"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0ea07ebf-c8ae-4a48-8d9b-d52b3306ce7a"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b1f61689-a23b-4d44-9dca-67dd6af91d32"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4813611c-1181-47c4-9c18-31c9fcdd20b1"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("659111c9-9531-4e93-80d5-84a755221022"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aff77d81-adcc-46b9-9098-dd23c9f2088a"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7cbbe133-86db-49c4-acf8-d49b70085327"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cb1c71e2-b4ee-4691-ab06-d11b1ae8625c"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("531edc76-5cec-473b-b6ac-497ddeac56be"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85bebf81-b731-43b6-a34d-167af14cf068"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("91c58c3b-6add-4d6a-8527-759d8277e36f"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b453b989-4dc8-4b45-974d-89f555015c7f"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 54,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6d6aa7ac-d1d5-421e-a9f1-8fe425e2dc13"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("94d50c2c-60a6-4152-abe1-0c6f8639419e"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("950d265b-0c37-49dd-9c74-8d7b204bdba6"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt", "Value" },
                values: new object[] { new Guid("31e970a9-f7ee-4486-9908-0f0fbd8252a8"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L), "1.0.1 (beta)" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("213f96f6-3298-4204-943d-c5b836378f29"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("70a39a75-1833-46b7-876a-9ebdcd161a12"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b1b4193d-964d-459c-bb4e-cb0e04a9e97f"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5934d727-1900-4db0-b546-05cbc4c63031"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2555cc81-fcb1-46f9-8460-6505e570d484"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("368ae714-c11c-43f0-b093-a49a2fcfa293"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("93ebc5bd-6bf4-4058-97f2-b72b8147ff4a"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5a61230-6b93-43a9-be44-88e8d1ade94b"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b157344-a116-461e-8e5f-328d54827a0c"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("23b946a1-6ff2-4b94-8acf-e0057c22bea5"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1d3cb28d-87a0-4f3a-99a1-0538b691d03c"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0ef75d81-ff39-4b4c-b668-7de947342f2c"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3b99a9b8-6931-4f60-9e54-013df13343a9"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bbfc072e-cc55-4844-956d-0f9fc9212912"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7470cfc7-cf31-454c-8749-8040d95c64ec"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("47da1d83-ad07-4d80-8456-0f064bc2a312"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("aa6a2329-4bb6-4f42-b14a-c42ef83f1a6a"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a2f6013b-8d9a-4371-93ad-555e6743d3a9"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4dfaf8ef-a9fa-419d-b3c1-3d18ca4a85a9"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e8435e88-9ffc-4ee3-8949-c13ef1d4e10b"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae367a88-3a6d-4c02-95e2-a2357be29938"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8890c36d-ef6c-45a8-9dc3-f497715e55fe"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2164252e-31fa-4bd4-a74a-56a5c900bfd6"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c5f11998-69e1-4093-be42-c2e9218ceedd"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("adb344c5-5218-47fe-9e3b-80fd7ae68d2b"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("24113793-b7a0-4beb-b0a4-f73f255096c1"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("14ae9e78-5310-4159-805e-4f5877f2318c"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("eb336670-afa3-4d8f-af09-517be7e1f219"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c8f1e967-551b-4802-b447-05b1ed8fbc7d"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e7944a8d-bf39-4dde-8516-faeff13a863e"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("10704115-83ae-42df-a542-0b0681e561e5"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e4da4d74-9cde-4cc8-ab91-b8fcbd48a311"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd3d46bf-0263-455f-8802-2d6534c18de6"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("031d0356-6d25-491b-bdbd-c0cda92d0218"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7fcb1315-bd40-4855-9564-12a01e9b859b"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0065481e-7345-4a24-8ee3-5cbac7b2dda6"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5cddd44f-f05c-453c-9ba6-fd870b80e08b"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("591f11fb-2b15-4edd-b124-f95b1ca255f7"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a045afda-2756-4d60-aac6-6bed508e95c6"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6a6a4377-6934-4226-bc39-71716d8f61fc"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ed6afb5e-e10c-4694-843a-3cdc9652240f"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cef48d57-3f40-4786-b4c2-23f4700df9bd"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("645e0474-7489-4661-99ec-6e3311fb6fae"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 918,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6efc0442-0d11-435e-b17c-9dd89a1006fa"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd0be6fe-2083-40d9-bbaf-f9069a4f5b99"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("12dfaa5a-159e-4dd2-b5fb-0bfcaf37ba98"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("224af850-3a00-4233-9e2d-a416f0fe054c"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1b7acfff-a83d-425a-bb8e-4a68e9a43518"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d60dd63d-823e-4654-90c2-1e34bada5f18"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4539c70c-35bd-47e9-84b1-ae13b4a5c6c6"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1fcb0e11-5dc8-4647-9bab-beff57d2c60b"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b52d06cc-f23c-446a-b2c5-b2c8ca4a5e65"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a7a9f11f-2cf5-45e0-ad1e-af067f0843cd"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a2dceaa9-8f9d-4b15-a93e-9967f090a54c"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7e69eceb-c6af-4a9a-8fa4-c3953597553c"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54aa6df0-7bdf-4261-90b7-65365f5285a3"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("394e001f-da2f-4786-bfe0-cacbef04628d"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1303,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c1637458-78d8-4659-be7a-d8e3389aa4ef"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1304,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ac5da157-6a1d-496a-821c-3fe73e7557af"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("07b5481e-5e0f-4c41-af91-7cbb32a71d22"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d0883299-3971-4a9a-a673-aab98a8f404e"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("70ba4874-fe8a-4802-a5f2-262d102c5d63"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4a6a0fa5-8b0f-4120-b080-6d9d42086d93"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2c0c64b9-1d2e-497f-b2cd-8b96604290b6"), NodaTime.Instant.FromUnixTimeTicks(17457662807598984L) });

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_Type",
                table: "Libraries",
                column: "Type",
                unique: true);
        }
    }
}
