using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexesForHistoricalQueries : Migration
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
                value: new Guid("a7f39352-fe08-462a-a408-014a9e01356d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("53fc5825-3ab8-437a-b288-dffb224cea76"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("18292749-47d1-4fab-8d1b-c90b024db207"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("10800e5b-f529-4e6d-a52e-f0fe1503a544"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("277ea002-30a7-4c93-89d3-f21f5e3e9db4"));

            migrationBuilder.CreateIndex(
                name: "IX_wets_150_dev_eui_timestamp",
                schema: "kropkharts",
                table: "wets_150",
                columns: new[] { "dev_eui", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_em300_th_dev_eui_timestamp",
                schema: "kropkharts",
                table: "em300_th",
                columns: new[] { "dev_eui", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_em300_di_dev_eui_timestamp",
                schema: "kropkharts",
                table: "em300_di",
                columns: new[] { "dev_eui", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_devices_dev_eui",
                schema: "kropkharts",
                table: "devices",
                column: "dev_eui");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_wets_150_dev_eui_timestamp",
                schema: "kropkharts",
                table: "wets_150");

            migrationBuilder.DropIndex(
                name: "IX_em300_th_dev_eui_timestamp",
                schema: "kropkharts",
                table: "em300_th");

            migrationBuilder.DropIndex(
                name: "IX_em300_di_dev_eui_timestamp",
                schema: "kropkharts",
                table: "em300_di");

            migrationBuilder.DropIndex(
                name: "IX_devices_dev_eui",
                schema: "kropkharts",
                table: "devices");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("98240ba7-0c4a-466e-95b7-4cd2d1aed64d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("35718ac9-e605-40c3-b3c9-85e9e0aa6500"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("069938af-d57f-44e2-81cc-19da8e4995d5"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("e3aaa4ec-02ca-4a48-b522-09712a2a3d33"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("75a380c6-865b-405a-aaf0-81947612a67d"));
        }
    }
}
