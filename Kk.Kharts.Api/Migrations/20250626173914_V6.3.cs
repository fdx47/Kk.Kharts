using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class V63 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DevEui",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "nvarchar(max)",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DevEui",
                schema: "kropkharts",
                table: "alarm_rules");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("d307177e-27b1-4ea4-b520-67e3c1ad7e6e"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("c50c9366-bfd2-46ab-bdff-c8caaa2e7518"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("39c7cbce-f047-4efa-b306-06470cc29b5d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("2051a4df-379f-46a8-a7de-43106e63b9f9"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("c8a91246-1b2b-4841-9a8b-8b2ce7f3a9a0"));
        }
    }
}
