using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v93 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "device_model",
                keyColumn: "ModelId",
                keyValue: 62,
                columns: new[] { "description", "model" },
                values: new object[] { "U502 Wet150 device with 2 sensors", "U502_Wet150_x2_Sensors" });

            migrationBuilder.InsertData(
                schema: "kropkharts",
                table: "device_model",
                columns: new[] { "ModelId", "description", "model" },
                values: new object[,]
                {
                    { 63, "U502 Wet150 device with 3 sensors", "U502_Wet150_x3_Sensors" },
                    { 64, "U502 Wet150 device with 4 sensors", "U502_Wet150_x4_Sensors" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "kropkharts",
                table: "device_model",
                keyColumn: "ModelId",
                keyValue: 63);

            migrationBuilder.DeleteData(
                schema: "kropkharts",
                table: "device_model",
                keyColumn: "ModelId",
                keyValue: 64);

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

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "device_model",
                keyColumn: "ModelId",
                keyValue: 62,
                columns: new[] { "description", "model" },
                values: new object[] { "U502 Wet150 with 2 Sensors", "U502_2Sensors_Wet150" });
        }
    }
}
