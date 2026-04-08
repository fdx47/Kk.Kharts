using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOneSignalToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "onesignal_api_key",
                schema: "kropkharts",
                table: "users",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "onesignal_app_id",
                schema: "kropkharts",
                table: "users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "onesignal_message_template",
                schema: "kropkharts",
                table: "users",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "onesignal_player_id",
                schema: "kropkharts",
                table: "users",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "onesignal_player_ids",
                schema: "kropkharts",
                table: "users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "onesignal_title",
                schema: "kropkharts",
                table: "users",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("aa46cf77-1886-4fc1-bcc0-1ee202e89133"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("8dcc7bbb-ac56-4cf1-9fbb-31a044ed45f3"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("83532803-775a-47c0-aa71-52cb4b3086a2"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("817326af-57c9-4690-8d07-4942b390ac53"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("520184fd-724f-4c7c-823f-618e186dad43"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "onesignal_api_key",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "onesignal_app_id",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "onesignal_message_template",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "onesignal_player_id",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "onesignal_player_ids",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "onesignal_title",
                schema: "kropkharts",
                table: "users");

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
        }
    }
}
