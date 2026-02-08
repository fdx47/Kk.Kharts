using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v98 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_devices_demos",
                schema: "kropkharts",
                table: "devices_demos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_devices_demos",
                schema: "kropkharts",
                table: "devices_demos",
                column: "dev_eui");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("d91315f7-c084-4a7c-9d00-ba4e5f6ab813"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("474f69e2-9ec2-477d-a0eb-96cab9796b57"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("64ec8e6e-a8d7-4991-97ff-56adaff83ec6"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("cd581b8f-d2d4-4c01-ad5e-d859dde0002b"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("ae5c28b9-694b-4981-a595-36c7cc9c42f5"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_devices_demos",
                schema: "kropkharts",
                table: "devices_demos");

            migrationBuilder.AddPrimaryKey(
                name: "PK_devices_demos",
                schema: "kropkharts",
                table: "devices_demos",
                column: "Id");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("36ba139e-2b02-41c8-9000-626f61b23773"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("7bc43623-ab54-4073-b6ee-617d11c2c377"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("c55b5c1a-5094-4769-909d-982e67f18ad0"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("5bbe8d05-c367-4853-af77-73fdc90084e2"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("c5a4a48b-7a29-41f3-988a-1867b8d2c2ea"));
        }
    }
}
