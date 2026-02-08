using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "em300_di",
                schema: "kropkharts",
                columns: table => new
                {
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dev_eui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    temperature = table.Column<float>(type: "real", nullable: false),
                    humidite = table.Column<float>(type: "real", nullable: false),
                    water = table.Column<float>(type: "real", nullable: false),
                    batterie = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_em300_di", x => new { x.timestamp, x.dev_eui });
                    table.ForeignKey(
                        name: "FK_em300_di_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "kropkharts",
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("d9273ad5-70ac-444c-a555-6ec56a2cf745"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("8acbfae2-0498-4d92-8d66-19ddc4668692"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("a3508b02-57ee-4f07-8e57-103a912b6941"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("f6447db4-c03e-430d-bb0f-22cb1f07e763"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("3bf3dbe1-e79a-46bf-97bd-3ce42a39c59f"));

            migrationBuilder.CreateIndex(
                name: "IX_em300_di_device_id",
                schema: "kropkharts",
                table: "em300_di",
                column: "device_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "em300_di",
                schema: "kropkharts");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("423ca722-9a6a-4a2d-92f6-f4462d5ea721"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("5fc083ca-7a47-4f38-b3d8-53a4e444f26d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("312001bd-9014-4d27-ab41-fe28e74b4ef5"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("6c951217-8999-496f-8a45-74a9a3aee0ba"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("10ae2955-3424-4a6d-9828-a93dc6c92a5a"));
        }
    }
}
