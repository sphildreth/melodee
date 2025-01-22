using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddedSearchMaximumPageSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "ApiKey", "Category", "Comment", "CreatedAt", "Description", "IsLocked", "Key", "LastUpdatedAt", "Notes", "SortOrder", "Tags", "Value" },
                values: new object[] { 916, new Guid("a91e47c6-3491-4299-8573-a61ab1936d17"), 9, "When performing a search engine search, the maximum allowed page size.", NodaTime.Instant.FromUnixTimeTicks(17375825700888760L), null, false, "searchEngine.maximumAllowedPageSize", null, null, 0, null, "1000" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916);

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
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7f3e6617-cc9a-4b6e-9cb3-ccc5d9bbc04d"), NodaTime.Instant.FromUnixTimeTicks(17373080253925050L) });

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
        }
    }
}
