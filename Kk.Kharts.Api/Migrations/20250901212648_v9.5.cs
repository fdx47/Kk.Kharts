using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v95 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "header_name_apikey",
                schema: "kropkharts",
                table: "companies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "3ctec")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<string>(
                name: "header_value_apikey",
                schema: "kropkharts",
                table: "companies",
                type: "nvarchar(57)",
                maxLength: 57,
                nullable: false,
                defaultValue: "DefautApikeyDemoKk2025")
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "DeviceId", "header_name_apikey", "header_value_apikey" },
                values: new object[] { new Guid("23a5cadf-a30b-427f-9e75-24a43c1ff142"), "", "" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "DeviceId", "header_name_apikey", "header_value_apikey" },
                values: new object[] { new Guid("9175b0ac-17c6-420c-a284-b1101958518c"), "", "" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "DeviceId", "header_name_apikey", "header_value_apikey" },
                values: new object[] { new Guid("7f1cc61c-017b-4cfe-88ce-478c92995fab"), "", "" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "DeviceId", "header_name_apikey", "header_value_apikey" },
                values: new object[] { new Guid("74587020-bc68-4571-ba35-51cd487f0409"), "", "" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "DeviceId", "header_name_apikey", "header_value_apikey" },
                values: new object[] { new Guid("834c7e8a-148e-46ba-b563-89cb16ffc55b"), "", "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "header_name_apikey",
                schema: "kropkharts",
                table: "companies");

            migrationBuilder.DropColumn(
                name: "header_value_apikey",
                schema: "kropkharts",
                table: "companies");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("71917538-6fc3-4e61-bb7f-471b7797ee78"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("f9bf22b7-bbe7-46a5-9944-7d20013e4df4"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("14d5fd73-a777-45e9-be3c-4fcdea378c5e"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("e09f91fe-588a-4ca3-b497-20b2962e2b57"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("81967336-a7f5-4d04-8522-55d3b9715ad9"));
        }
    }
}
