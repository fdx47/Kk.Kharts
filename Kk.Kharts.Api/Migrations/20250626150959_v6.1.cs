using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v61 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlarmRules_devices_DeviceId",
                schema: "kropkharts",
                table: "AlarmRules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AlarmRules",
                schema: "kropkharts",
                table: "AlarmRules");

            migrationBuilder.RenameTable(
                name: "AlarmRules",
                schema: "kropkharts",
                newName: "alarm_rules",
                newSchema: "kropkharts");

            migrationBuilder.RenameColumn(
                name: "Enabled",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "enabled");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "SensorType",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "sensor_type");

            migrationBuilder.RenameColumn(
                name: "PropertyName",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "property_name");

            migrationBuilder.RenameColumn(
                name: "LowValue",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "low_value");

            migrationBuilder.RenameColumn(
                name: "HighValue",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "high_value");

            migrationBuilder.RenameColumn(
                name: "DeviceId",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "device_id");

            migrationBuilder.RenameIndex(
                name: "IX_AlarmRules_DeviceId",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "IX_alarm_rules_device_id");

            migrationBuilder.AlterColumn<bool>(
                name: "enabled",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "sensor_type",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "property_name",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<float>(
                name: "hysteresis",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "real",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_alarm_rules",
                schema: "kropkharts",
                table: "alarm_rules",
                column: "id");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("e89238f8-abe3-4b82-a7c3-ebd82d7e0db0"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("c31fed77-1d7f-492a-a1ca-65f58c906b96"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("84780d2c-5904-48df-ab44-beff7a4c1b38"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("6606e017-60db-4bf9-bbcf-3d583c63230f"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("782a62a0-2b01-452c-93e0-6056bcf3a2ff"));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alarm_rules_devices_device_id",
                schema: "kropkharts",
                table: "alarm_rules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_alarm_rules",
                schema: "kropkharts",
                table: "alarm_rules");

            migrationBuilder.DropColumn(
                name: "hysteresis",
                schema: "kropkharts",
                table: "alarm_rules");

            migrationBuilder.RenameTable(
                name: "alarm_rules",
                schema: "kropkharts",
                newName: "AlarmRules",
                newSchema: "kropkharts");

            migrationBuilder.RenameColumn(
                name: "enabled",
                schema: "kropkharts",
                table: "AlarmRules",
                newName: "Enabled");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "kropkharts",
                table: "AlarmRules",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "sensor_type",
                schema: "kropkharts",
                table: "AlarmRules",
                newName: "SensorType");

            migrationBuilder.RenameColumn(
                name: "property_name",
                schema: "kropkharts",
                table: "AlarmRules",
                newName: "PropertyName");

            migrationBuilder.RenameColumn(
                name: "low_value",
                schema: "kropkharts",
                table: "AlarmRules",
                newName: "LowValue");

            migrationBuilder.RenameColumn(
                name: "high_value",
                schema: "kropkharts",
                table: "AlarmRules",
                newName: "HighValue");

            migrationBuilder.RenameColumn(
                name: "device_id",
                schema: "kropkharts",
                table: "AlarmRules",
                newName: "DeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_alarm_rules_device_id",
                schema: "kropkharts",
                table: "AlarmRules",
                newName: "IX_AlarmRules_DeviceId");

            migrationBuilder.AlterColumn<bool>(
                name: "Enabled",
                schema: "kropkharts",
                table: "AlarmRules",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "SensorType",
                schema: "kropkharts",
                table: "AlarmRules",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "PropertyName",
                schema: "kropkharts",
                table: "AlarmRules",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AlarmRules",
                schema: "kropkharts",
                table: "AlarmRules",
                column: "Id");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("5ca19182-a20b-4675-bc08-8229ffef0883"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("fa0e7f1e-a765-4251-a7af-451c3c60e109"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("bd747315-97de-41e2-a80b-3a096489a055"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("e1a5381e-2880-4cfb-9626-af3c368afd64"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("d4bde507-47d8-4bcc-9188-0d9465f5ac9e"));

            migrationBuilder.AddForeignKey(
                name: "FK_AlarmRules_devices_DeviceId",
                schema: "kropkharts",
                table: "AlarmRules",
                column: "DeviceId",
                principalSchema: "kropkharts",
                principalTable: "devices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
