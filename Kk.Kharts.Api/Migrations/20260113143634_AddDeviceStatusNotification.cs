using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceStatusNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "device_status_notifications",
                schema: "kropkharts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: true),
                    dev_eui = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    message_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    sent_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_status_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_status_notifications_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "kropkharts",
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("bce51847-45ed-4958-a961-f1d76c6d14ee"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("892d9565-761a-4928-b24f-697a55d7ac8e"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("427ac466-790b-45e6-95e7-720f147c400c"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("10b5b855-ab88-442f-a240-276ab4012de5"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("1b3ed00d-f406-4b83-b9a7-308387862751"));

            migrationBuilder.CreateIndex(
                name: "IX_device_status_notifications_dev_eui",
                schema: "kropkharts",
                table: "device_status_notifications",
                column: "dev_eui");

            migrationBuilder.CreateIndex(
                name: "IX_device_status_notifications_device_id",
                schema: "kropkharts",
                table: "device_status_notifications",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_status_notifications_is_active",
                schema: "kropkharts",
                table: "device_status_notifications",
                column: "is_active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_status_notifications",
                schema: "kropkharts");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("67a53df4-672c-4005-b099-da71ed1416ca"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("44a660e9-7e22-41f2-b6a7-f037eca4afdb"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("ee4941cb-73b6-45aa-938b-d1c7900bc56d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("7be98d8b-4a15-4f97-ac44-a08e18f0e6f7"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("ef1060ef-335b-402f-874b-7b740130155e"));
        }
    }
}
