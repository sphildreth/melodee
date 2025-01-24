using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddedJobConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a6cb2056-54e5-49e6-9744-a447fafbb0dd"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dc08a5e0-11c3-4701-886e-f6f2a1dc84a6"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("847f6687-ff9b-4762-ad40-3a4d42e80bc2"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("541c3fc7-bb51-4228-ae64-dfdd7af87de4"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ed525003-b6a9-476f-9b52-bc07746bdaf4"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5f469c98-dff6-4a59-ae33-02da1a874a97"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1e4cdefb-4191-42e6-8d59-27d95fdf6161"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("735f0d3e-ad1d-452d-b364-aadfde625b2a"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("da568a5b-ef37-490d-b039-8f4ce79e576c"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("24bdcd1e-e266-4ba2-9723-eb01482ce486"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e1e9aa27-c73b-4c75-8478-34292464176b"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7e6e6f5f-e3c9-4be6-8141-3d3e22b76ea9"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4a07ca17-faaa-4630-976f-a8c6025119d8"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1f2d01ce-2943-40e9-8930-d9c2cc2aa1d3"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b15f69bb-3dcb-4a0f-b01c-3b5c1f6c0612"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95808688-ba3e-4db5-9855-4ad45ff632b8"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("20fc037b-77d2-4855-9aac-d1d4ae758f8b"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7a667055-a24a-4b50-872e-7d6ac7a20e37"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("366f7c0a-e1c7-4aa7-b243-cd0ebed9bf61"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ad7f0901-478c-4441-a27f-32fcd8456ea4"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9b7102eb-76c7-4f9e-85e3-aea30ccbfdaa"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b053851-718b-4bdb-96aa-4c9fce45e8e2"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6bc60e38-a63b-425f-9567-834cda583fc2"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a0e29206-a090-4828-8826-029914e7695b"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("34404cff-2772-4a45-8491-3053c6736ac5"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cce67e17-de01-41fb-a40d-95d2466671d8"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fc081957-3a7a-4c0e-aea0-b7e9193f0ae1"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8c811fac-e40a-413a-9568-1a9af6971afc"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("87244725-d23a-43ea-82e9-b95bae1d8c4b"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b25cce8a-8fc1-4df3-b23f-52230363cdc1"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2898e485-f8c7-4f4c-95bb-6724dca27fa6"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0ea1e6e9-81fb-4b0a-a2f9-4e5ec14daa8a"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("756d4195-e109-4dd6-a5a6-e624a65921de"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e92a65a6-0caa-4bc3-a3b8-fa83a45c3856"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ee406090-0bd6-4256-8f82-dcaff4f00805"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1d3f2930-4d61-4acc-b6aa-1fb288aed4a3"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af441884-8509-4f90-a324-341e17cfa57f"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("661f1f84-48b4-4744-9ac8-19f85c763e2f"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("810e9e26-2343-440c-95f8-cc887da34e55"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6ad85993-2cce-4ea3-82f0-9a87524cca1c"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("db2bdfab-78a0-4a82-91cd-bf1f99e7e196"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("70f40777-fbdd-41ea-84bc-e363e59be13c"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5c7ae1ce-4328-4c14-b3de-ffaf9ee2b172"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bbeb95e9-0d06-4171-8816-54c832cba8ec"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f1472dbe-43c0-4ae8-9754-6eef01f667f2"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9db2861d-c891-42ac-aca7-1d50af2f93fc"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3c4db498-2387-4028-b138-4de9d4b19cc1"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3d59076a-6fca-4ebf-9627-bf2f7dabf1c4"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c89ffa3d-d06a-420e-aa9b-5811dcff528c"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6dc18c39-65eb-4800-a541-0c5f117c216f"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5272db4d-9ccb-4f6a-8ca0-639c52326196"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("796f9566-ae17-4837-828e-c0bf41cf8148"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f018c272-05c3-4ac5-896f-877826edcc4b"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ea1c43f-a48d-425e-8ce2-482a36a727ac"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("56949456-5fbc-4951-9212-989f968390e9"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dfcca85b-a98f-4dd0-a292-b75e3e13329e"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2240e261-c3ad-4087-9d13-3b312947b8ba"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bff2445c-0e2d-4f0d-974c-884ea12486ac"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("57228d55-6efd-44a0-8e69-1cb549be9bf6"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cc98307a-666e-445d-a54b-9e1bf0051699"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a7cf31bc-c8e1-4fdd-94ba-e5aadfefd2b2"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bfcb82bb-7ad3-4929-af08-6d2250efb42a"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("32126160-4d33-4df1-8b16-04d88c971977"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5a4107b4-1fe0-463e-b709-4f00e8b5798c"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("711dd1c3-d5e8-4bf9-86bf-c1ce51746166"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a52a6f6-7ee4-48e1-b53a-feac372d898a"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c9defb16-82b4-406e-a308-6d9c944e16ae"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a592ba1f-fbfd-4f2e-ac42-12f541b75610"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6765ec1b-8df7-4950-930c-a5cdaa25367c"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bc4dab65-dbad-4d2f-ab67-46899a15a17c"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f9020338-5950-4ee5-a7df-a69d1573d520"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3bd01d37-665d-4d00-83ca-3cb17a33c6b5"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1084d8a4-b950-42e4-a893-6b1a4b5892d4"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1d477b45-a284-432a-84c4-e707639f0402"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0f488d8a-6357-44d9-8cbb-6fe9f5168f9f"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5e4efdb1-4821-4482-aff9-1f0ff7c48ac6"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a68add83-2f83-40ab-a589-2fe31e297e61"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b6928003-4734-4d77-94e5-e12dbf5d9006"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e38ccad5-6364-4b96-ae0b-965d441962ea"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("26e38a67-5f4c-4a10-8af9-c73c4b6dd373"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a665face-ed71-4899-9c9f-93f2712816dc"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("19a1699a-7768-4783-848c-8649ede29665"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("037665c9-7369-4704-995a-15d5b20e7f98"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0f22e39e-67c8-47e9-9172-6f7949f34028"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e0985c65-d389-42ea-ba42-1a6cc0ff0b95"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("86f4bc26-13ef-4a31-84f2-9cebac35681f"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b4b62ea9-e355-4769-ad99-156e5d68a0e1"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b2a5a5c-ce23-4257-bbc6-1041530549e7"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3c80dfa8-5849-4d9d-a346-d98f0772961a"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("71e0f06d-156c-400b-9f3b-f2a405bfd264"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("82d50295-1096-4bc3-8ea9-52a34c25b802"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5404083c-0187-45d6-8b86-4a0f0c22e7e2"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[,]
                {
                    { 1400, new Guid("faf5d8d1-8e48-4d16-8bb1-e3fb88e58427"), 14, "Cron expression to run the artist housekeeping job, set empty to disable. Default of '0 0 0/1 1/1 * ? *' will run every hour. See http://www.cronmaker.com", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L), null, false, "jobs.artistHousekeeping.cronExpression", null, null, 0, null, "0 0 0/1 1/1 * ? *" },
                    { 1401, new Guid("47c341a0-8751-416b-b990-ca1902fedaa7"), 14, "Cron expression to run the library process job, set empty to disable. Default of '0 0/10 * 1/1 * ? *' Every 10 minutes. See http://www.cronmaker.com", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L), null, false, "jobs.libraryProcess.cronExpression", null, null, 0, null, "0 0/10 * 1/1 * ? *" },
                    { 1402, new Guid("06681855-c92f-4083-a2d8-17375ea7fa72"), 14, "Cron expression to run the library scan job, set empty to disable. Default of '0 0 12 1/1 * ? *' will run every day at 00:00. See http://www.cronmaker.com", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L), null, false, "jobs.libraryInsert.cronExpression", null, null, 0, null, "0 0 12 1/1 * ? *" },
                    { 1403, new Guid("2c51f74f-0530-444d-a137-d7878acaf66c"), 14, "Cron expression to run the musicbrainz database update job, set empty to disable. Default of '0 0 12 1 1/1 ? * ' will run first day of the month. See http://www.cronmaker.com", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L), null, false, "jobs.musicbrainzUpdateDatabase.cronExpression", null, null, 0, null, "0 0 12 1 1/1 ? * " }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403);

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fcfdab56-a37e-445f-92af-47a478a010ef"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("132ddb1f-8c50-4d74-b920-dd50fa8eaa3b"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7e4e999a-b2f4-45c6-bdef-433923f0eb20"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b6383d6a-d517-4cd2-ac0c-82ea80bc5605"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("481649d4-2db4-46b3-a97e-9d1b60347bdc"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0413b9d8-ec92-41f3-bc10-981bfcca078f"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6e9f7413-0fb5-4fbb-b39d-f44068b80af0"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53070f82-0666-410e-98fd-d731453ed876"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("16de5038-d91a-4929-9c26-a535caa738dd"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("43b5c804-2aab-40b8-af9e-65e365df00f8"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("52be8225-81b2-47be-a6b0-6a3e4e1aab59"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca7372c8-9241-4f4c-a973-16968bf7a7e3"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("35bee40a-49ee-420c-a234-01393514511f"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("47f818d8-7ec2-407b-98f7-ff29a2704b2a"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5de7d99c-3b10-4111-b31a-0948cf426764"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6a9a9e59-5112-4aea-9cdf-cc652fbb7f29"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2247a04d-4a7c-4ddf-9661-c9f5f7b73c80"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("463a611b-5a17-4c74-8f06-76255b0cd6c3"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b1254b21-2dac-4666-ae29-fbf3594a2b38"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5db43bed-7157-492b-a6e1-fe1b418a1734"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ffdb71e2-64fb-40bc-8a34-a98dc0a9561a"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51f5bd0a-5382-49d5-bec0-6cd9383667ca"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("77d6cd3c-20b0-481e-bee9-65f3e5b2ecde"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ba43c0d4-822a-4fdc-9945-034e22f30329"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9fa579d0-0a73-4e06-abe4-3e5deaffff01"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d76524c1-6a44-43ba-9b98-8b892e337239"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("816b8f6c-33c0-4140-9faa-93a0099c6142"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("815df500-35f4-4a63-a442-52a9e7e711c7"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2f52eb39-64de-45a2-86f3-b68299e3ede3"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fec507ce-30f4-4194-95aa-c4bcf258398b"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("92855471-0412-4bc5-90a0-9bdfabc8be7c"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e46cc924-33a9-4858-b4a9-bc8f4cbe035b"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d3e149c8-6fb4-4c5b-b0c8-ba802c234059"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("81dfb4a4-a669-42bc-83b7-94205b8a08fc"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("43dd4f09-2de7-4708-a4aa-236a7a0f144f"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("71f129c4-aed2-442d-b402-cc8844d2b892"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("554878b2-343d-4727-bcee-1879824458f0"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("27d925db-66db-46a7-afc7-d1d1f49cd182"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8bd0757d-a873-409c-bd8f-42caf157a034"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("93b520e9-75c0-4b1a-987b-f328df9ca068"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("166e5ed8-ca09-4476-a3cf-5f4fd05bb1da"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("58ceec6e-f546-470a-8d4e-6cf8b826e691"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e4485fe6-c32e-453b-a84a-b98af7f4b289"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("15696616-d656-4cde-85f5-a6491d451e0d"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("417dd22b-10dd-4a38-b5d2-af82e2f0d2f4"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6b1bb112-a5ce-4f7f-bed2-7eea6c0716e5"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d20d1966-539e-482c-a04d-62dd59d9136e"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8a53a4b0-d143-4a90-b873-61b182fe12ea"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9c8a0a42-1334-4e09-a778-6680ea5a2d3b"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a149b81-ff65-4f09-86ec-5af16ace4903"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e0988c07-952c-4cd6-9d89-d8524e98a000"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("97419029-25e8-4dcf-ade9-4445462603c2"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21411d84-e491-4bea-a9f8-b25a8271f921"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b1f92685-1da4-4548-abcd-295849f14a38"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("adc07cfd-5c75-49d4-8cec-cae06bd3254e"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8ae5278c-9b72-46b4-92e0-30744bff4dfd"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e4a00111-db57-49f6-ac08-a10331c6da9c"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b50b409-4bc6-4d21-ba8d-a85ea6df542b"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d44efe2e-ac8d-4bfa-a1f0-fb361ba41bce"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0d6ec429-1f17-46c6-a9e4-fc9cb6302b98"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2b878ec0-1188-4560-b47e-bffdf6013581"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("94311bd1-7589-45aa-8f07-3ace170412df"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6847adbb-e039-4229-a968-a8bda2e6187a"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("08dbc1e7-cd29-44b8-873d-759a41a94e72"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("417e98d2-46e3-4baf-a135-f7c91a18bcc0"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ed2b99b1-fe93-427b-902c-0ee989ba5ae2"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b52003f4-601c-4973-8a9c-f5996d1506fc"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4e4e7a07-1d2d-4340-9747-d087bf8147b1"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("50294334-6572-4b29-89f5-c78fa19930d5"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0e63a24e-0406-4673-ad8d-eb8fa22b3e60"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0dd729ad-9a84-4432-b508-c526344a1e0f"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e0d8edcb-887b-4e10-a6c7-1084edb13699"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4f71565e-3234-41e1-877f-f8267d188a01"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("672bd1ff-c785-4733-af3d-1a604dc75b72"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("879ca732-68ff-4468-9f33-0a520ab72d12"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("83e8a0be-54e1-4d2b-aeae-4dbedb0053b7"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("87984e40-33de-4479-94a2-27169ea84983"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("41f56803-a5b3-408d-8e0c-dbae412f1830"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a91e47c6-3491-4299-8573-a61ab1936d17"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("623cf740-0fb9-4154-a2f6-1a8a3ea52860"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("36d8aa4b-7cc0-46af-9343-d27c31aa2f57"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("08142d91-6a89-41a1-a5a0-6e1f4a69c09c"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c943498e-0235-42d6-a8fe-678bf61e4ea9"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("706ba529-41ac-4b4a-90b6-32460f6de12c"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6a64cb92-a742-4a16-ba99-23ca5db71804"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("92a5a521-db33-4602-9d9d-8773595e71e0"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("098638bf-b528-41b6-a5c7-e2f5879fc5e6"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f2685b31-e8e0-4659-80ca-c5e8af3ad332"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b530e7af-17e5-49a2-923c-834ec97c38ae"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("800e22e2-6de0-4619-94d7-e7921be101b3"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b7dec802-7dd4-4df5-a6e8-0b1862d69797"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fdc53674-ef73-4e4d-a514-86958d96b9cb"), NodaTime.Instant.FromUnixTimeTicks(17375825700888760L) });
        }
    }
}
