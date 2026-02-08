using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v91 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "kropkharts",
                table: "companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "DeviceId", "IsActive" },
                values: new object[] { new Guid("64b6e6f2-d5ef-4ca0-bb83-ad8040f67775"), true });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "DeviceId", "IsActive" },
                values: new object[] { new Guid("2aff387c-7785-432f-8877-57220f230320"), true });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "DeviceId", "IsActive" },
                values: new object[] { new Guid("2afb597c-4393-41a2-94b4-870e355de31b"), true });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "DeviceId", "IsActive" },
                values: new object[] { new Guid("f99dfaf9-5747-4393-aeb6-a68844e470d4"), true });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "DeviceId", "IsActive" },
                values: new object[] { new Guid("a4525d9d-a086-4873-8acd-5d4624da88de"), true });

            migrationBuilder.InsertData(
                schema: "kropkharts",
                table: "soil_parameters",
                columns: new[] { "Id", "A0", "A1", "Epsilon0", "Name" },
                values: new object[,]
                {
                    { 7, 1.06f, 6.53f, 4.1f, "Custom 1" },
                    { 8, 1.06f, 6.53f, 4.1f, "Custom 2" },
                    { 9, 1.06f, 6.53f, 4.1f, "Custom 3" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "kropkharts",
                table: "soil_parameters",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "kropkharts",
                table: "soil_parameters",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "kropkharts",
                table: "soil_parameters",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "kropkharts",
                table: "companies");

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
        }
    }
}
