using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPins2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPin_Users_UserId",
                table: "UserPin");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPin",
                table: "UserPin");

            migrationBuilder.RenameTable(
                name: "UserPin",
                newName: "UserPins");

            migrationBuilder.RenameIndex(
                name: "IX_UserPin_UserId_PinId_PinType",
                table: "UserPins",
                newName: "IX_UserPins_UserId_PinId_PinType");

            migrationBuilder.RenameIndex(
                name: "IX_UserPin_ApiKey",
                table: "UserPins",
                newName: "IX_UserPins_ApiKey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPins",
                table: "UserPins",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f3c9fa06-4f44-4b36-943c-e92f82a7e5a6"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dc63b5b1-e338-48db-86c8-e3d79e13cc4c"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1a134ea3-0050-4906-af6c-7ca9301624c5"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bca96f91-cff3-4fa6-b135-c5d537b0b816"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9ee6eb83-668e-4dc1-89e9-e889280dd999"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1dbd33f6-818a-4ac9-9700-5241587360b5"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85d10f4e-96d0-4cb7-a76b-83c90de01860"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("77363b6d-75bc-4fd0-aba1-c4d491ca8a19"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae607783-5e4c-4229-a05e-a61fc6991218"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25dcfd03-89ec-497b-87f5-9668744adde7"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f8fe61c4-21e8-4588-ac8a-d41d51bd8cd0"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1051707e-8d4f-4969-b1ec-80261b03ca01"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("578f6161-7f37-472c-aa5a-5c9808daf6b2"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("041af7bd-109a-4291-8f78-19588d18df86"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("134fb529-74df-4ab5-8812-2022e91c220d"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5e42b087-9e31-4d19-9303-6002f3b5ff74"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f70c7f17-20f0-4a5e-841c-6df39d57c39a"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21365019-5003-4de8-94f7-fde53c13c91d"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8c9d6ffa-4b17-4e78-be32-518659927779"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f96a0ac3-5725-4cc9-97bc-c3e6425016d7"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8342b445-697d-4725-b6e7-2185f4a70433"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("88e9c025-5446-4f62-b20b-d2797d0eec28"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a914b35b-17ac-4275-a604-eb8c3629858c"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53315970-10af-4ade-abdd-30ec13e3882e"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("006e0adc-c5e8-46d1-8177-a0e319c09d08"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("97981314-c045-41cc-a809-1ef14aef04ab"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("25cc740c-fd72-404e-ad61-e03e857707a3"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fca351a7-2169-43fa-a37a-82c4a86384e8"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7baa2261-f2c9-4e85-a8a0-710b173d0b42"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("95c49ca0-e665-4b02-ac11-0170a352b075"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1e669421-72ba-4ffe-be54-f115572bc858"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("16332327-af9d-45df-bfa2-8d9cb97c8c75"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("179c41c4-9c9d-425d-acc5-8c48bc1f22c7"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd33e705-6bb0-45b0-92f2-c38283065b76"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("316d17ef-a267-4aa1-8d51-c8109e73f123"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("dbc7a3cf-2866-4077-b891-3cd143008410"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6a251b61-e9c0-47a8-b0b7-daf9dbe96e5f"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("41384e9a-2f36-4489-9e3a-4b37232d3c69"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e5d2b0ae-7328-4499-b221-2cc6ad287f63"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b3bbe87a-b24a-4932-a122-0b2bf87e234e"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("488b895c-f11b-490f-98f9-5de08f4f6cfe"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c3c60060-1b96-44ae-9961-d99d3ab9f28d"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fd40e915-28d2-4790-a5e6-ae0843f6c8db"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("51b3136c-11ce-443e-9855-4048a166ed23"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53f5c0d6-57f4-4fce-9ca1-e6c8f1416c3c"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1267d22a-526b-423c-b48e-3561134ee5bc"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("66866916-c43b-4f35-8058-e69a7ccd059e"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("627cadbb-9f5c-471a-aeba-4aa629e83b21"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ab0aa10e-b8bf-4353-9f92-996f66230d69"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ad8e1017-3fae-4cc8-876c-0b5532880ad3"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b3bfdf60-e619-4190-a781-dc85996bc0c1"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e766bf22-26c6-4d71-87d2-ee7fa7ebbe00"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("102f1079-097c-4385-a409-9095bc3e791a"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f5286359-df34-4ca5-8fc1-e995f249fc40"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8758687e-56fa-49a5-8327-c8744c485453"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e128c28a-1656-4582-aa32-83fd83a895b4"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d64258f5-fab7-4ad3-9266-74cbe0370166"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("c4e8b867-87b3-44c4-84d0-f3a7f3f06d27"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("68e30223-d00b-4a33-94dc-9d1d3230c919"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("706a4254-6aee-43f7-afcb-4c9178a51ad6"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8a562cae-64a0-48c1-b091-ea53d84040f4"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4f588a51-c320-4e50-bcf3-b333c818117d"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1c5f7f74-055b-4f60-8a7d-605b7ddd8855"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a7f9fd1e-65be-4b31-8ca9-c31e260c27c9"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f843a589-2828-4ecf-9335-3a2dbfc93706"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("01a8a68b-f7c6-4728-81f0-f9df3a2113e3"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d9f2f8a2-040a-4a86-89a8-9dec24855d3d"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ed813c54-0d81-4f40-98bc-81b1fa2fce4c"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("803fd583-bf83-48eb-9001-30eab400eed5"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2d03fb99-662e-42ff-b68d-77462c7ecce6"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b11435e0-2b77-48a4-ba59-1fe8394b3018"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9cea4339-9603-4068-ae1d-1e5f99b157d2"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bb05e6b4-4ee4-4641-8511-3db5fcbf58d0"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2937a12f-ed0f-456a-ad64-c76cc097f01c"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f43c5e86-e83f-4c05-b060-69a6eff875b5"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 917,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("089042e5-f124-4d26-b311-00337bc2ff99"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("53f515ef-7ad9-44cd-9c96-1972599ec9fd"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2ba44339-596d-4ac1-90f3-53ee4b550fc5"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a85f506d-291d-4962-b396-0f5aef288951"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("349ef8b2-4a4e-4f83-b56e-b77db0971e7f"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1f1c176b-ed02-4930-8f01-2f0dd565ac5a"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3b8f95c5-9f30-48af-b7a7-0724d7baf155"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("468f9217-8f28-4962-a330-6d3ac5487fb3"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8edf16d7-9818-41e9-8624-f4106d41bffc"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9e25b86f-8edf-46e5-b053-2cedef94645d"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("80304b88-72b7-4eae-96c3-08b48a31f6d5"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("bbd2502f-5890-40b2-baa0-864319e0ed90"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("db8f14b1-acbc-4d92-86dc-0198beacec32"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e132afa1-2b8e-4663-adb3-8d6955729ce6"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9a640db3-c82b-4cdf-8294-2c4c1bc493cc"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1ea287d0-221f-4992-9fe2-ca60e288bdad"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5d580a4d-940d-485b-b3a1-d16fb9194d37"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5902f010-eeb2-46f7-9d6d-dc0276d7b960"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("10cd8417-3631-4e92-859f-69233bc044eb"), NodaTime.Instant.FromUnixTimeTicks(17399223956951321L) });

            migrationBuilder.AddForeignKey(
                name: "FK_UserPins_Users_UserId",
                table: "UserPins",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPins_Users_UserId",
                table: "UserPins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPins",
                table: "UserPins");

            migrationBuilder.RenameTable(
                name: "UserPins",
                newName: "UserPin");

            migrationBuilder.RenameIndex(
                name: "IX_UserPins_UserId_PinId_PinType",
                table: "UserPin",
                newName: "IX_UserPin_UserId_PinId_PinType");

            migrationBuilder.RenameIndex(
                name: "IX_UserPins_ApiKey",
                table: "UserPin",
                newName: "IX_UserPin_ApiKey");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPin",
                table: "UserPin",
                column: "Id");

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
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("472790cd-56b7-45d1-857a-75899c01933b"), NodaTime.Instant.FromUnixTimeTicks(17399222854772831L) });

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

            migrationBuilder.AddForeignKey(
                name: "FK_UserPin_Users_UserId",
                table: "UserPin",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
