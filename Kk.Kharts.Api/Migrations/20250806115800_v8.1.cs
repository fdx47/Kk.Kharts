using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v81 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "em300di_parameters",
                schema: "kropkharts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    ValuePerPulse = table.Column<float>(type: "real", nullable: false),
                    water_conversion = table.Column<double>(type: "float", nullable: false),
                    pulse_conversion = table.Column<double>(type: "float", nullable: false),
                    alarm_threshold = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_em300di_parameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_em300di_parameters_devices_device_id",
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
                value: new Guid("90842bde-2169-44f9-95c8-72da5b715123"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("b5ee2f57-8ccb-40c3-8727-71f5015eed73"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("91ae6010-09cb-40eb-a17b-bfc38126c3a4"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("9abc7aff-265e-4d58-b298-9871c2cba698"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("57810dea-0b17-4f25-b7e1-733db6bfb385"));

            migrationBuilder.CreateIndex(
                name: "IX_em300di_parameters_device_id",
                schema: "kropkharts",
                table: "em300di_parameters",
                column: "device_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "em300di_parameters",
                schema: "kropkharts");

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
        }
    }
}
