using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Melodee.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserIsEditor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEditor",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d208a10-8533-4f8e-9f6b-fac6084027ac"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("92ea01b8-c000-4567-8799-bab06a49cf3f"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0829dead-04c4-4aaa-b7f9-9446c968d481"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Libraries",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f710c160-c81c-41f3-8d1d-2064ee13fee9"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1b14affc-231e-4055-ab52-2885fbfb612a"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("85bbd4ef-1f52-4970-a41b-23eb7988d153"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ddb0abeb-1232-4e1a-801f-ded14b5e782e"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fe2a0c29-7cf1-415f-8586-d8fdf55edc4d"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a88b1077-97ed-42f7-8a26-49e4a926dcd8"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("96c7fdfa-0a1c-4f54-bfde-ba5e0db8122f"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 26,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("533dc4ba-a620-4534-a325-5ae0526fa110"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 27,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3ed93ee8-9854-422d-9547-4fba775bfba5"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 28,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2af809a6-8f3c-4378-9d9e-218913cfe1bd"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 29,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a3391052-2cbf-4d54-aa51-2cc056a1526e"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 30,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("adc1a5e0-5e77-4a59-9e6d-1c8b16d16021"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 31,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1f70a9d8-baa6-4b05-bfbf-b24604f24407"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 32,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("e36f4d79-82f0-4343-ab1e-44e386676577"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 34,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("096c4637-28d5-464a-bd3c-fd5a66163d39"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 35,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f22716fc-e3cc-405e-acfc-6ebabb0cf97e"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 36,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fa1fbfd6-dc3f-434b-a76a-16a16ccff982"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 37,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("418061f4-d4ec-40d8-b4b0-32d12f161c5e"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 38,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("626acde4-122f-488a-a6bf-521cb7f272f5"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 39,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a5e14b98-c37b-4f9b-92af-3c81e5a11e5b"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 41,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("96c6723b-9808-4c6c-8644-aa2ffb8f17ee"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 42,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ecfa5b4a-8b27-4d82-a6d2-6e7e844e0fa7"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 43,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0fbf296c-52e1-4b41-adf7-54a2803a27ed"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 44,
                columns: new[] { "ApiKey", "Comment", "CreatedAt" },
                values: new object[] { new Guid("d5326650-bb75-4944-8042-dbed5491a19d"), "The maximum value a media number can have for an album.", NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 45,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("915a317e-9925-41bd-9e55-9b9d3cfca69f"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 46,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("731eb087-f05f-4406-a576-56137b30140d"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 47,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("36aad946-12d3-4c47-bd3f-6ae210ef872e"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 48,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a77a916-41b6-4554-a7eb-821243fe4289"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 49,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f5adc23c-592e-4d90-978a-3b15aeeed9da"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 50,
                columns: new[] { "ApiKey", "CreatedAt", "Value" },
                values: new object[] { new Guid("4527188d-a05e-4b28-a5e8-82d8d1e8e2f7"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L), "_duplicate_ " });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 53,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2eaa8079-e608-4718-8a03-a7aa8ef72eae"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("306c4273-139a-4334-a75b-ba3fc5555539"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d4845976-dd3e-4904-a515-3b4d060290b0"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 102,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5354a8ec-160f-468a-9033-c60a49c4c91a"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 103,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("33ee9aa6-4087-42e7-8a1b-3e263f30416e"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 104,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4aca5b28-db06-4c66-b66b-5e47f31f0306"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a21c78bb-98e0-422c-b0b2-9cd5af45445a"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("2236b816-16bb-4ea3-8eb6-9d62428e3d35"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("97d62274-0c4a-4f06-9b16-e991c3c2c2b9"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("18724bee-18be-43b8-a27c-c91b9b2f7336"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 300,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4ae306f3-3660-4810-a6d6-d54873772280"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f5c1a617-e0da-4d18-9932-2ae2f8a2e1ed"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 400,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a1853c88-e2b2-4315-a938-2da46f7519e2"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 401,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("42a97605-f465-4f5a-be50-6aacc0cb1a80"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 402,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("621d5b1a-2d89-44ad-b0fd-b42b35379c47"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 403,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fdcf3e85-9380-4057-8bf2-7e77531c9c10"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 404,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1aa101f3-3cd5-426a-940b-4fc4fe3d37b8"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 405,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("6178647f-6cb3-4a3c-bbc6-4ba572a9f149"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 406,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("d0acf454-88cd-40f2-ade4-f638d32ecf6d"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 500,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("a0f42077-96fa-4adb-8a78-66d12173d484"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 501,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("47330615-5664-4add-9268-dec1e249cdb2"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 502,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("3f2bf914-6a25-47c3-b7c2-d399097f5df3"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 503,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("009ae652-3af0-4ee4-91eb-5b7d2ce0bec6"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 504,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8c913edd-516f-4e77-9745-45283f5b6f84"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 505,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("cf1845a0-4a55-484e-b2cf-4fb0373d577b"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 506,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("fddd95aa-8057-4e40-814f-882b70026b90"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 507,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("87b17fe2-70a0-4795-a659-b44e34362dd6"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 700,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("4d6c5aec-307f-4729-a8e9-768256e4a647"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 701,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ae57b51a-3c90-451e-ad09-2ed06ed28645"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 702,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1936a0ec-3528-46fb-8f24-fe70ffa1192e"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 703,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("ff034270-4955-4d8b-bd17-3aaac06c1f6f"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 704,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("428b6366-8f2c-44e5-968b-560d580aa8ce"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 902,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("46cb99c0-1807-47a6-97c2-4b21eef2713b"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 903,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("544384db-b761-438b-9813-64808c9ccf7e"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 904,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f220bcc2-ab49-4b84-9176-ebf87f1a873f"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 905,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("b095e169-562b-4fac-a3b1-71d7c574ee94"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 906,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("1a4e9ae9-f18f-469f-9e52-15824e4e2800"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 907,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7edbdd65-844b-40c8-8e80-36e6d0e18837"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 908,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("21cda5b8-2665-4535-8b16-6367e22a387d"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 910,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("104a283e-827a-4b03-b87a-5c675dcbeb76"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 911,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("27169226-3f9b-44e0-a2dc-763be3c47636"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 912,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("62d1f04e-075e-4b13-a098-072000bf1a98"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 913,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("0a6357d6-2974-46d8-b5c6-cd2daf9a980c"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 914,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8de031ad-e6bb-430c-880c-c781826e7b5c"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 915,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9a3dc6d2-8aca-4dfb-bcc0-ffc3d89b7bcb"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 916,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("eb80571c-28c1-4460-9b83-aa4dd651b3ce"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1000,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("8d70050c-25d3-484f-a637-23fcb5e6afd5"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1001,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("f65b9853-b0b8-4940-bfa7-c67a836dd752"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1002,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5bfd1c19-c9ae-4097-8cd1-0a47137402ca"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1003,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("76072b8a-bbb7-4fd0-ab93-061432c26c75"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1100,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("af797038-8614-49e9-8c49-2712e6bb9a0c"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1101,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("eea4c5ec-9da8-45eb-9f01-9db01d807e60"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1200,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("7bbe0d18-a7ce-40c8-b959-8d554619db60"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1201,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("60f4170b-d3bf-4d6c-82b6-4d5ec08d0c24"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1202,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("144bb827-c780-485b-a77a-d1683251681c"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1203,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("9d5e3ab0-1848-4185-b462-81018ccf5107"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1300,
                columns: new[] { "ApiKey", "Comment", "CreatedAt" },
                values: new object[] { new Guid("352f5a08-cdab-4e8c-b6e4-978680375d89"), "The maximum value a song number can have for an album.", NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1301,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("41d57fe0-5230-4894-9372-30748895826e"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1302,
                columns: new[] { "ApiKey", "CreatedAt" },
                values: new object[] { new Guid("5d0c8fc4-8357-48ef-8751-8aed68ad9049"), NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "Comment", "CreatedAt" },
                values: new object[] { new Guid("8e91d9af-ce82-4127-b71a-7d77c4a126e3"), "Cron expression to run the artist housekeeping job, set empty to disable. Default of '0 0 0/1 1/1 * ? *' will run every hour. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17385167004153895L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "Comment", "CreatedAt", "Value" },
                values: new object[] { new Guid("cfbd44fb-ad5d-43d9-b0c8-69e4532e83a7"), "Cron expression to run the library process job, set empty to disable. Default of '0 */10 * ? * *' Every 10 minutes. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17385167004153895L), "0 */10 * ? * *" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "Comment", "CreatedAt", "Value" },
                values: new object[] { new Guid("725f6cdf-05ba-4e56-9847-6a13dc13c183"), "Cron expression to run the library scan job, set empty to disable. Default of '0 0 0 * * ?' will run every day at 00:00. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17385167004153895L), "0 0 0 * * ?" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "Comment", "CreatedAt", "Value" },
                values: new object[] { new Guid("bc6a1930-5d8c-4e77-bd63-1a2f5064b3d0"), "Cron expression to run the musicbrainz database update job, set empty to disable. Default of '0 0 12 1 * ?' will run first day of the month. See https://www.freeformatter.com/cron-expression-generator-quartz.html", NodaTime.Instant.FromUnixTimeTicks(17385167004153895L), "0 0 12 1 * ?" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEditor",
                table: "Users");

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
                columns: new[] { "ApiKey", "Comment", "CreatedAt" },
                values: new object[] { new Guid("fc081957-3a7a-4c0e-aea0-b7e9193f0ae1"), "The maximum value a media number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

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
                columns: new[] { "ApiKey", "CreatedAt", "Value" },
                values: new object[] { new Guid("756d4195-e109-4dd6-a5a6-e624a65921de"), NodaTime.Instant.FromUnixTimeTicks(17377593245196778L), "__duplicate_ " });

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
                columns: new[] { "ApiKey", "Comment", "CreatedAt" },
                values: new object[] { new Guid("71e0f06d-156c-400b-9f3b-f2a405bfd264"), "The maximum value a song number can have for an album. The length of this is used for formatting song names.", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

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

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1400,
                columns: new[] { "ApiKey", "Comment", "CreatedAt" },
                values: new object[] { new Guid("faf5d8d1-8e48-4d16-8bb1-e3fb88e58427"), "Cron expression to run the artist housekeeping job, set empty to disable. Default of '0 0 0/1 1/1 * ? *' will run every hour. See http://www.cronmaker.com", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L) });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1401,
                columns: new[] { "ApiKey", "Comment", "CreatedAt", "Value" },
                values: new object[] { new Guid("47c341a0-8751-416b-b990-ca1902fedaa7"), "Cron expression to run the library process job, set empty to disable. Default of '0 0/10 * 1/1 * ? *' Every 10 minutes. See http://www.cronmaker.com", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L), "0 0/10 * 1/1 * ? *" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1402,
                columns: new[] { "ApiKey", "Comment", "CreatedAt", "Value" },
                values: new object[] { new Guid("06681855-c92f-4083-a2d8-17375ea7fa72"), "Cron expression to run the library scan job, set empty to disable. Default of '0 0 12 1/1 * ? *' will run every day at 00:00. See http://www.cronmaker.com", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L), "0 0 12 1/1 * ? *" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1403,
                columns: new[] { "ApiKey", "Comment", "CreatedAt", "Value" },
                values: new object[] { new Guid("2c51f74f-0530-444d-a137-d7878acaf66c"), "Cron expression to run the musicbrainz database update job, set empty to disable. Default of '0 0 12 1 1/1 ? * ' will run first day of the month. See http://www.cronmaker.com", NodaTime.Instant.FromUnixTimeTicks(17377593245196778L), "0 0 12 1 1/1 ? * " });
        }
    }
}
