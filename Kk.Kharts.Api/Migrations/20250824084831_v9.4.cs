using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v94 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_devices_dev_eui",
                schema: "kropkharts",
                table: "devices",
                column: "dev_eui");

            migrationBuilder.CreateTable(
                name: "wet150_sdi12_metadatas",
                schema: "kropkharts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DevEui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Sdi12Index = table.Column<int>(type: "int", nullable: false),
                    Sdi12Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sdi12InstallationLocation = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wet150_sdi12_metadatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_wet150_sdi12_metadatas_devices_DevEui",
                        column: x => x.DevEui,
                        principalSchema: "kropkharts",
                        principalTable: "devices",
                        principalColumn: "dev_eui",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("71917538-6fc3-4e61-bb7f-471b7797ee78"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("f9bf22b7-bbe7-46a5-9944-7d20013e4df4"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("14d5fd73-a777-45e9-be3c-4fcdea378c5e"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("e09f91fe-588a-4ca3-b497-20b2962e2b57"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("81967336-a7f5-4d04-8522-55d3b9715ad9"));

            migrationBuilder.CreateIndex(
                name: "IX_wet150_sdi12_metadatas_DevEui",
                schema: "kropkharts",
                table: "wet150_sdi12_metadatas",
                column: "DevEui");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wet150_sdi12_metadatas",
                schema: "kropkharts");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_devices_dev_eui",
                schema: "kropkharts",
                table: "devices");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("bee7e205-1451-4907-8075-b6c45d6650df"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("7ab5ed1e-6e84-4674-ba86-35f1aeba50bf"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("608c5129-b5fd-4af3-9b37-7a1ff5280e5c"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("ebc8fb33-5d7c-4494-a030-08a52fe17442"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("1c7c5f11-c3f3-4ea8-bf07-8ae4d9817525"));
        }
    }
}
