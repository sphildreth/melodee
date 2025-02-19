using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserPin",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PinId = table.Column<int>(type: "integer", nullable: false),
                    PinType = table.Column<int>(type: "integer", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedAt = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    Tags = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Notes = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPin_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7aff1073-c154-439e-b455-38223cc3026a"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a99098ff-16c3-4f60-9995-319276b884b3"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f1899e5d-4841-4473-bba8-2d106f900844"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5bcc65a-fc86-4148-be27-c3574f54a3ba"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("72b0ee97-e9e5-4d3a-97f9-1d81f28fda21"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("467a45c4-6a1c-44d9-a01b-182bf2e347e6"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2a15d055-4785-4a4a-b6d1-0194dbed648a"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d6eb22af-cbed-462d-8b84-fc0aa2b39174"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a7ee3ab3-3c8a-4d28-a8b6-32eff4e5f473"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("779f690b-dc38-4f81-9b17-bd36d0b839b3"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d1f60181-4f6b-4345-989b-b7bd756abd4b"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a7ae9da-5505-44b5-8602-787b3ae4ab56"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("05557a7c-09bf-4e59-b0ca-58fe8beb891f"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("41a3db9e-77db-4a55-8f90-696e62588079"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1006cf83-ea2f-4802-86bc-fed58ae34e8c"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f81a8abf-8264-4dff-90ed-28bc52274e53"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ade153a5-8276-4c34-8fce-90f3404d49b8"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ed83632-f594-4229-935e-9317a66d490d"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cc8048ac-584e-4cb6-b201-dd15fbf84f61"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("75582ad4-de4d-421b-a54c-1fc813a480e2"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0558f66e-2bb6-4cc6-b818-aed55863b341"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("323071de-fa92-48fc-8ce1-b7a36ff7dcdd"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("69fff3eb-ed7a-4d0d-aac7-92f14813a1bc"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c6aa835c-9600-471a-bca9-d35ab4654580"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("16d3058f-3457-4c0b-bdaa-36a94b162c81"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b8a76cda-6de2-4b1d-b96e-e65d9664f1ff"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b355c21b-5a73-4eaa-9e5c-878615de6a7e"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ff095304-1987-4896-9705-681dc56aa3f8"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("852745ea-f982-4f22-9fed-4bd91b403eb5"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d0f46850-f78e-4135-b27b-ec28de279df3"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7ef46246-ba4a-4744-b3e7-68c4ed881369"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c23573b-4f2f-432d-97d7-f21e973113bc"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cdbc87c1-0746-4f89-a0c4-220a9060eb63"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("87a2fb29-ed0c-45db-bea0-016cd538163e"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("591391f8-df5f-4d7e-afc1-78ef3d042ff4"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c14ec9d-3128-4eb8-b419-81b19f99d30c"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9365d7d2-bdad-47d1-9aa0-d801ea5a1a21"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bcbb34af-a006-4676-b584-978d9405c6fc"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ada1fbdb-9fff-4ef5-bc27-c10852f464c2"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a31a082-80a4-4147-a004-128a3cfd2035"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ecee524-197d-40ec-9fe4-b54e81d2b6e1"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5bbed4c2-0b52-4d66-b4e3-60b5fef725dd"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d51d1a2e-18f0-4ef3-8e73-25e3df82ae8f"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ed14b834-9451-41cc-8470-d6ebd8135430"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d28ba788-1867-4d56-96d2-4b874e4b5dc8"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("69a112eb-a808-44fa-a0f5-e1d0dbb1ee00"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e70e08ec-65ee-4648-928b-85e1c511dd7b"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e3cfa1a7-5c27-446f-9629-a59fa19ec612"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1a08d66d-e12e-4a9f-a126-56d3d7db525f"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3486e39c-516c-4c4d-8e1b-26be8cf05e88"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d07c521d-531d-4236-9104-f7bfdf83f151"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e40082c3-8833-4904-af55-754a434bdb44"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f187765d-d8a2-4923-9d17-3e6795f0e1df"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d519ccc-1a11-4ff6-a39a-859b0b0275f4"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1ce8e83-5e28-465a-98a6-418769bf24de"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1eef470b-66d4-4477-a467-e76ff71e523c"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b33c0544-6e44-4a74-a163-1737c48468b1"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("78735548-4f98-44aa-9199-a14759a6b032"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd943297-487e-4d2e-8505-d4616889fa45"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c07b3937-3bb9-460a-9f1a-2869a4ccabbf"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3642cbe0-baff-4c94-9cc5-ce9fcef4b3d5"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2c4ce3f2-0ea6-4a35-a3be-ca35a92b30ce"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af2d169b-277b-40a6-bb01-d08bef772b80"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("eef3c9e7-3583-4bfd-8142-73d05ad708b1"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6257ac75-d2ac-449b-81f9-93650c896e1a"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d3aa1106-6fab-4257-8757-dc06f0dcec12"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ec9a5917-3a69-449b-b48e-f79bcd49a543"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1ac53e6-82ed-4d13-93ad-08346c2abc7d"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7f8079a1-8c26-4972-98e0-8fea3381a25f"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b155c227-a19d-4946-9f6d-653179054dc0"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f6638107-5e1f-458d-ac0a-de8b72476b18"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ca4a474-555e-424d-bbab-c0e363e48594"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("44d65b04-21e8-4b38-bab0-c8b8f8da002e"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54a5b128-1d50-498a-ba7c-90155dd6c7d5"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85ad093e-3271-4c15-9fea-645b05e57cce"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5a77e9ee-f0eb-4638-b447-d909444069ed"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt", "Value" },
                values: new object[] { new Guid("472790cd-56b7-45d1-857a-75899c01933b"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L), "true" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("00e8e01f-172d-44a5-afca-d9f664024559"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8c9fce95-f0d5-4da8-8740-630cbd48f358"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("68b4955f-f329-43ae-b7ed-b8fa039e02e6"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b0dc0822-fd51-4681-8fc6-40c89d129c0a"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5a7a5aaa-4abc-42b4-b093-8885e0ec3ce5"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0b9aac8e-c8eb-4538-b8c7-f71696549e9a"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f4e7d3c5-0286-43b7-8d2b-24d99c26d4fe"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d546e972-6c73-4505-9d1d-1b406284a494"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a136947b-e56a-48e7-9dae-2205d012e2e6"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ba1eea2c-31ac-4a58-b0a3-c95d5526e5d4"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cf705a22-23c8-461b-bce9-66a1477bf6e6"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95423e34-3328-47ff-b02e-65f06b19143c"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("83f95d77-20f8-43ae-b9b6-63afb96110bf"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("99bb35cc-7d3e-452a-ae39-07093ad6281a"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3e491652-8d9d-4a86-a747-1e40efba3eb4"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ed683b72-a18e-4029-8d70-73055cd8b0dd"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e6c8d5a9-f975-407f-8095-a339e3f8cbce"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

            migrationBuilder.CreateIndex(
                name: "IX_UserPin_ApiKey",
                table: "UserPin",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPin_UserId_PinId_PinType",
                table: "UserPin",
                columns: new[] { "UserId", "PinId", "PinType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPin");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a7d0993b-992a-456c-bf1d-fbcd7625396c"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bf06be32-c95f-4a5c-b241-752daef0478d"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0bc1f88c-11ea-4750-9a34-5262473bb0ec"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("27bcb519-d9fd-4b59-97f9-82289b5be4b6"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c19e1e36-6e24-405f-a62f-01a9ffb0a819"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("57ea2ca9-f69e-47ef-8dd3-0d0169c06641"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51c48263-2e2c-4983-b2ef-0885ba28b825"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ca83a05f-29c9-4496-bcf4-d423bd7b6cdc"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f4ba33f8-19b1-43a2-96e9-c85dd38fbaf3"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("05f0efbe-033b-4f15-ae14-141eace3320a"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("73204192-be65-4336-8308-cd85e62d6c81"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0df52524-3a13-4ddc-8485-1cab72d53303"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("52f8190e-6b04-4882-9963-e5d30903351f"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c0fdf045-dcdd-4c0a-bb13-64dc55324438"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b30c6aa3-0e2f-4e9d-99b1-4b2f384e95bf"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("68684128-a20a-4ff4-bb95-3c7b87413079"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8fe93224-7b71-4fb2-b17d-e93df6d279d5"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a07c7cec-67a2-489e-b8d9-c95b7a696d61"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7d2eeefc-f8bc-4e0d-914c-0c143ba5681a"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("145568f9-9f69-4b47-8462-589540e089e9"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8e41511f-5e92-4b23-b947-bc04c995afc2"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b40fcf93-f38b-4bd1-9718-dc27e69d0b70"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9acbd105-a4aa-4297-b4ba-7b5b5e00aaae"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("58eab599-70c5-4e01-9491-374bc5f940b3"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fe82fba7-a616-466c-a414-619eda2f0f73"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4b87fa8d-77d1-4825-8957-03084f959a78"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("05b1dd4a-c821-4022-be7d-d95f02c20196"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5460f9e6-425f-4aea-ac55-e98e6f55ef86"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1cff105e-34e2-408b-858b-e422edca3db7"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("875b53cc-2302-4400-8389-eb5406b69832"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4c6739bc-61c8-4c66-920b-cc135f87d7bf"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cc495ed2-bbad-44f3-bb51-6f547b36a5f5"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ece978d-9447-4058-919c-39592a74276c"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f56ad731-8b43-40f4-84d4-a5032320dafb"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("232447a6-f4d7-43f9-ace1-154167c7fab3"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("78fbec1a-7618-4c02-b25b-66584048274c"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3a2c2907-4e07-4322-aa97-a3514165f864"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("901a9ec0-8f52-4544-b725-e92ba2373f9c"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("682c1e5e-04a9-4ef0-935a-7258490d1fb2"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("74b2825e-3538-4d23-842e-041424df1a12"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cba92f59-00e1-4ba9-9e7e-fbc097b5f856"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95791adc-da25-44ea-9113-21ad610760aa"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d86c90ad-7515-4516-b46f-d0474231144c"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0ae4be96-bab9-4646-bce2-4e1985a8bd94"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ff1f0dc9-8194-4d14-906b-a4354cddb7d6"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a2b77237-941a-43fb-9692-7a6973c0f598"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51de0ffb-e287-4b57-8a05-f45575ec5353"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f2a68d3e-dd6c-4563-9769-09959634c599"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8ab4a64c-0d02-440f-94a2-ec5fdaad35b9"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e308f17a-9bbe-4f8d-8dcd-b3f558579c3c"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd577639-7f89-413c-9c0f-70526a9d4c20"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9c96bfb9-f23d-4156-93c1-c4365187e883"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("add0ef4c-7a53-4807-afd6-1127f3f517fa"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("54374002-9e27-4d2d-ba98-95e7c0872f95"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("13c8563c-2b39-4366-b53d-75d3d4cc5523"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4371ab77-1572-4c33-8bdb-a4906ad3de8e"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0fcb90aa-8766-497c-91ae-132caba18bbc"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0d6683f6-d308-4639-a4bb-d5ff56edcae6"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8b62c0c2-fef3-42db-9a5b-30f825a6cd9f"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("242613aa-3fc0-47e9-9d50-356603ad21c1"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5de830d8-b292-45c6-9bf1-d235de3fdda3"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9c38eb9d-6847-403b-bb55-c23ab235ed2a"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a30b39f-9fe3-492f-a980-c8dc313e707c"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e8a1765-2be0-46df-93da-ea772c299a3c"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ebad197-6fc2-454c-92f7-50c16bf2c84d"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b3acd683-1e94-4f75-aefd-59fe23e633ac"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4623b714-d3ea-4611-a0fb-842cdbfb287b"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c92d47dd-6f4f-4be4-aba8-1174ec6efedb"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6e699c20-51c4-4c06-8fe9-11aa750a6588"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("be3fa34e-cf40-4ec7-b25d-3ae658d1a3c8"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("185eb3f8-62c6-4ec0-ad0c-7797839e6dba"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("018e7cf8-6db0-445b-b766-978ca43673da"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7ef3799b-24cf-4e29-aba8-b43cb51835ec"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c479de34-818d-4564-9f89-9560bd8ffcb4"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8054da9c-5bd0-43a4-8cef-2fae86f30bfd"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("60e38c97-4bb8-4867-bfff-40be4c8b96c4"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt", "Value" },
                values: new object[] { new Guid("8f59432d-f5a5-44f5-a937-e5ef37b0192a"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L), "false" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("eb1c90fa-a548-4d8e-a013-11dac51a02cb"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f0c04a47-7ee9-4669-b629-47e04dcc04a1"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("153003c8-8f6e-47fc-975e-5337dbf3899a"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("77d09e6b-685d-4f5b-ab6b-d5cb2739caf6"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("22425a3a-85be-4524-a30e-813f6c34d9c3"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8128acee-90e9-458a-8178-ab1b68fcf331"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d55149f3-0b78-4bdc-b403-8faa86832a46"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3e4cd1a5-7c52-4fc7-87c9-c29f62d0a17b"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7e468a9e-f700-4d5b-91fb-663cab09b275"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bd97ecea-f5ff-42d7-8179-14a76af44541"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("204605c7-d29d-4e5c-98fb-2b4f0a9c4a67"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("207d0318-0349-4428-99b9-c8f7ac25ea28"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("03ad6c13-132b-43c5-b8f5-6e3b4fa9aa9a"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("68647ce0-97ac-4e67-81b7-68da8dd41fd1"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7dd4d448-802d-4b14-82c1-e75c893c0ec2"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1cbc9d8-e407-4dc5-84ad-f86812434182"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("71dc2fcd-00e6-4094-8112-4610d47a309b"), NodaTime.Instant.FromUnixTimeTicks(17389554776280915L) });
        }
    }
}
