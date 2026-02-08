using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v97 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "devices_demos",
                schema: "kropkharts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dev_eui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    name = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    description = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Installation_location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices_demos", x => x.Id);
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devices_demos",
                schema: "kropkharts");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("acdd4eca-17c3-4360-8d52-8b1b0ea287b2"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("ae7de867-ade6-417b-abd1-3a65a50af36a"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("35858304-8519-414c-958f-1c87a0331a80"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("6838502f-f61e-4a23-b5cb-8c5bab487141"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("012dd74e-79fc-4073-b061-08f7b98f8288"));
        }
    }
}
