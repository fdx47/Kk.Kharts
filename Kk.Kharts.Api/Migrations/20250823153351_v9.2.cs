using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v92 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "wet150_multisensor_2",
                schema: "kropkharts",
                columns: table => new
                {
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dev_eui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    sdi12_1 = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    sdi12_2 = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    batterie = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wet150_multisensor_2", x => new { x.timestamp, x.dev_eui });
                    table.ForeignKey(
                        name: "FK_wet150_multisensor_2_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "kropkharts",
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wet150_multisensor_3",
                schema: "kropkharts",
                columns: table => new
                {
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dev_eui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    sdi12_1 = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    sdi12_2 = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    sdi12_3 = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    batterie = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wet150_multisensor_3", x => new { x.timestamp, x.dev_eui });
                    table.ForeignKey(
                        name: "FK_wet150_multisensor_3_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "kropkharts",
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wet150_multisensor_4",
                schema: "kropkharts",
                columns: table => new
                {
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dev_eui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    sdi12_1 = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    sdi12_2 = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    sdi12_3 = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    sdi12_4 = table.Column<string>(type: "nvarchar(19)", maxLength: 19, nullable: true),
                    batterie = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wet150_multisensor_4", x => new { x.timestamp, x.dev_eui });
                    table.ForeignKey(
                        name: "FK_wet150_multisensor_4_devices_device_id",
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
                value: new Guid("7f365e75-18f0-4c13-99c6-76e3681ab322"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("d28cca3b-25a5-4a1a-a4bb-3aa992272d5c"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("af8fc035-fc66-446c-8682-66e7ab03bad9"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("07b077d5-7d06-4ea6-9789-e23ddad0843f"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("acd50c7e-285c-4b53-b05f-ac10449fc1d9"));

            migrationBuilder.CreateIndex(
                name: "IX_wet150_multisensor_2_device_id",
                schema: "kropkharts",
                table: "wet150_multisensor_2",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_wet150_multisensor_3_device_id",
                schema: "kropkharts",
                table: "wet150_multisensor_3",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_wet150_multisensor_4_device_id",
                schema: "kropkharts",
                table: "wet150_multisensor_4",
                column: "device_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wet150_multisensor_2",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "wet150_multisensor_3",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "wet150_multisensor_4",
                schema: "kropkharts");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("64b6e6f2-d5ef-4ca0-bb83-ad8040f67775"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("2aff387c-7785-432f-8877-57220f230320"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("2afb597c-4393-41a2-94b4-870e355de31b"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("f99dfaf9-5747-4393-aeb6-a68844e470d4"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("a4525d9d-a086-4873-8acd-5d4624da88de"));
        }
    }
}
