using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v65 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_rules_devices_device_id",
                schema: "kropkharts",
                table: "alarm_rules");

            migrationBuilder.CreateTable(
                name: "user_alarm_rules",
                schema: "kropkharts",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AlarmRuleId = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_alarm_rules", x => new { x.UserId, x.AlarmRuleId });
                    table.ForeignKey(
                        name: "FK_user_alarm_rules_alarm_rules_AlarmRuleId",
                        column: x => x.AlarmRuleId,
                        principalSchema: "kropkharts",
                        principalTable: "alarm_rules",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_user_alarm_rules_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "kropkharts",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("a4c1d6bb-fe2c-42e2-9ddb-1f613db7491d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("4824360a-f086-4044-a439-06d0baf5f714"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("315a478a-818f-4e7e-abeb-3518ae509aa3"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("c8eb222c-fdf6-40b5-b07b-32d57ab64c97"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("de2ec3fc-8745-46d1-9b86-5481bc7a9d3e"));

            migrationBuilder.CreateIndex(
                name: "IX_user_alarm_rules_AlarmRuleId",
                schema: "kropkharts",
                table: "user_alarm_rules",
                column: "AlarmRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_rules_devices_device_id",
                schema: "kropkharts",
                table: "alarm_rules",
                column: "device_id",
                principalSchema: "kropkharts",
                principalTable: "devices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_rules_devices_device_id",
                schema: "kropkharts",
                table: "alarm_rules");

            migrationBuilder.DropTable(
                name: "user_alarm_rules",
                schema: "kropkharts");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("6a3a6a2b-1fe4-4f57-969d-748428cf9796"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("278d1e01-a44d-4464-901b-b84b346e8d09"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("d5b1aa16-9d08-4d38-9109-56a45bdd3728"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("b7093d8f-0ef9-47ce-bd14-1531ca206f70"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("662818b6-81a5-48e8-99f9-90a2b227e2a9"));

            migrationBuilder.AddForeignKey(
                name: "FK_alarm_rules_devices_device_id",
                schema: "kropkharts",
                table: "alarm_rules",
                column: "device_id",
                principalSchema: "kropkharts",
                principalTable: "devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
