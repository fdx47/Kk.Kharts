using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class _67 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_alarm_active",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "is_alarm_Handled");

            migrationBuilder.RenameColumn(
                name: "sensor_type",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "description");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("39b7de65-a2df-46da-ab4a-082c519d3bd3"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("dc0fe1a4-145e-4a2f-a0ba-06847f445647"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("b7281edf-d7e4-4b4b-941d-dabf29b74da3"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("e0a4e29f-1518-4118-9985-7e2b80b6ddd0"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("735b989c-1a2c-4ca8-b16f-a5966c27b5f8"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_alarm_Handled",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "is_alarm_active");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "kropkharts",
                table: "alarm_rules",
                newName: "sensor_type");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("a2ab197a-c1cf-45a4-876a-625c1353e612"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("4efedf5b-4de6-4a7f-9651-94a3d1bbd437"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("838ddced-79ee-4c63-8d60-7de10f6cf750"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("8f8b3ea5-2546-494b-b511-3dc2cc9c97ec"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("c5bae9d9-a3d6-41fc-bcff-bc49ec9147b5"));
        }
    }
}
