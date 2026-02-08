using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v90 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "soil_parameters",
                schema: "kropkharts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    A0 = table.Column<float>(type: "real", nullable: false),
                    A1 = table.Column<float>(type: "real", nullable: false),
                    Epsilon0 = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_soil_parameters", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("c1dfafc6-85c6-4292-8257-52e64258536b"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("6807829e-aafa-4bfe-8065-d4836743c7d7"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("abc7c7a8-a183-43da-a6b8-c3522de08aba"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("75b875d9-9095-4bb1-a2f3-5d93e87a5c78"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("27c0fd5b-175c-4891-8758-013a606cff34"));

            migrationBuilder.InsertData(
                schema: "kropkharts",
                table: "device_model",
                columns: new[] { "ModelId", "description", "model" },
                values: new object[] { 62, "U502 Wet150 with 2 Sensors", "U502_2Sensors_Wet150" });

            migrationBuilder.InsertData(
                schema: "kropkharts",
                table: "soil_parameters",
                columns: new[] { "Id", "A0", "A1", "Epsilon0", "Name" },
                values: new object[,]
                {
                    { 1, 1.6f, 8.4f, -4f, "Minéral" },
                    { 2, 1.3f, 7.7f, 5.5f, "Organique" },
                    { 3, 1.16f, 7.09f, 1.8f, "Mélange de tourbe" },
                    { 4, 1.16f, 7.41f, 0f, "Fibre de coco" },
                    { 5, 1.04f, 7.58f, -0.3f, "Laine minérale" },
                    { 6, 1.06f, 6.53f, 4.1f, "Perlite" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "soil_parameters",
                schema: "kropkharts");

            migrationBuilder.DeleteData(
                schema: "kropkharts",
                table: "device_model",
                keyColumn: "ModelId",
                keyValue: 62);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("0d3c3522-2c4f-47c0-bfea-c71ff5ccb06d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("e34ed9d4-41f0-4f54-ae89-c3d2bce93f9d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("7f083e77-ba95-43c8-b53d-154bb23a3a09"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("57791bf0-7de8-408b-a744-0aaeb824f6cd"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("b71ef813-ae5e-43eb-9b25-5d9a843b9b87"));
        }
    }
}
