using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class V64 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DevEui",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "dev_eui");

            migrationBuilder.AlterColumn<string>(
                name: "sensor_type",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "property_name",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100)
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<float>(
                name: "low_value",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<float>(
                name: "hysteresis",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<float>(
                name: "high_value",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<bool>(
                name: "enabled",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "device_id",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("Relational:ColumnOrder", 1)
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "dev_eui",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 2);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "dev_eui",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "DevEui");

            migrationBuilder.AlterColumn<string>(
                name: "sensor_type",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<string>(
                name: "property_name",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<float>(
                name: "low_value",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<float>(
                name: "hysteresis",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<float>(
                name: "high_value",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<bool>(
                name: "enabled",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "device_id",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1")
                .OldAnnotation("Relational:ColumnOrder", 1)
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "DevEui",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16,
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("c498bc8e-8411-49f4-8421-8ba499c6650b"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("54924feb-57b0-41bb-9a6c-376d8d131b4b"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("9e6948ab-4e29-48ef-98ef-1a00cf4a3b87"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("33a3a9a7-1039-4d7b-9a95-14c08bd62542"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("8edbf229-1f6e-4787-8917-0d12a308de48"));
        }
    }
}
