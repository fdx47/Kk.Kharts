using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v96 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Installation_location",
                schema: "kropkharts",
                table: "devices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AddColumn<bool>(
                name: "has_comm_alarm",
                schema: "kropkharts",
                table: "devices",
                type: "bit",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 12);

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

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 1,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 2,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 3,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 4,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 5,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 6,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 7,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 8,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 9,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 10,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 11,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 12,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 13,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 14,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 15,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 16,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 17,
                column: "has_comm_alarm",
                value: false);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 18,
                column: "has_comm_alarm",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_comm_alarm",
                schema: "kropkharts",
                table: "devices");

            migrationBuilder.AlterColumn<string>(
                name: "Installation_location",
                schema: "kropkharts",
                table: "devices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("23a5cadf-a30b-427f-9e75-24a43c1ff142"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("9175b0ac-17c6-420c-a284-b1101958518c"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("7f1cc61c-017b-4cfe-88ce-478c92995fab"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("74587020-bc68-4571-ba35-51cd487f0409"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("834c7e8a-148e-46ba-b563-89cb16ffc55b"));
        }
    }
}
