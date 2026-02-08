using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "telegram_linked_at",
                schema: "kropkharts",
                table: "users",
                type: "datetime2",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 16);

            migrationBuilder.AddColumn<long>(
                name: "telegram_user_id",
                schema: "kropkharts",
                table: "users",
                type: "bigint",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder.AddColumn<string>(
                name: "telegram_username",
                schema: "kropkharts",
                table: "users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 15);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("3b331317-a343-4257-a44b-971620fee54d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("279d5225-5763-454b-b57c-ac709e19395e"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("3ebc5beb-9037-42e1-bf47-f5bf0cb7b028"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("64683ff8-9e6e-4622-a1bd-54e831c467a8"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("a2ba9bf6-c394-4b3a-81c4-9844e059964a"));

            migrationBuilder.CreateIndex(
                name: "IX_users_telegram_user_id",
                schema: "kropkharts",
                table: "users",
                column: "telegram_user_id",
                unique: true,
                filter: "[telegram_user_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_telegram_user_id",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "telegram_linked_at",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "telegram_user_id",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "telegram_username",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("3ee7e8b6-3026-4c85-b745-2af2fa1fd3c5"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("5f11efd1-6703-485d-a810-a9752a67fb6d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("96ab14a4-aacc-4554-bc3c-fac3786e9d8f"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("7466b485-d6fc-4acb-b762-703844dbdcd3"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("8ee1951e-9fb1-4b80-b9e9-91b78e4fc284"));
        }
    }
}
