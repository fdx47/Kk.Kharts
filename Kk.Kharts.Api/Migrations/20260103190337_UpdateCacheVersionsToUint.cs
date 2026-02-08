using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCacheVersionsToUint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "cache_versions",
                schema: "kropKharts",
                newName: "cache_versions",
                newSchema: "kropkharts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "kropkharts",
                table: "cache_versions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<long>(
                name: "local_storage_version",
                schema: "kropkharts",
                table: "cache_versions",
                type: "bigint",
                nullable: false,
                defaultValue: 1L,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "indexed_db_version",
                schema: "kropkharts",
                table: "cache_versions",
                type: "bigint",
                nullable: false,
                defaultValue: 1L,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "cache_version",
                schema: "kropkharts",
                table: "cache_versions",
                type: "bigint",
                nullable: false,
                defaultValue: 1L,
                oldClrType: typeof(int),
                oldType: "int");

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

            migrationBuilder.CreateIndex(
                name: "IX_cache_versions_updated_at",
                schema: "kropkharts",
                table: "cache_versions",
                column: "updated_at",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cache_versions_updated_at",
                schema: "kropkharts",
                table: "cache_versions");

            migrationBuilder.EnsureSchema(
                name: "kropKharts");

            migrationBuilder.RenameTable(
                name: "cache_versions",
                schema: "kropkharts",
                newName: "cache_versions",
                newSchema: "kropKharts");

            migrationBuilder.AlterColumn<DateTime>(
                name: "updated_at",
                schema: "kropKharts",
                table: "cache_versions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<int>(
                name: "local_storage_version",
                schema: "kropKharts",
                table: "cache_versions",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 1L);

            migrationBuilder.AlterColumn<int>(
                name: "indexed_db_version",
                schema: "kropKharts",
                table: "cache_versions",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 1L);

            migrationBuilder.AlterColumn<int>(
                name: "cache_version",
                schema: "kropKharts",
                table: "cache_versions",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 1L);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("5d2fa407-938b-4055-8a35-e7eb502af51b"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("b03d910a-7652-458a-b590-d69ce6511fb0"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("923438b2-5dfe-42b2-8799-9c23d5da3a5b"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("a68490be-d4a0-48a5-9261-beb4d26e8f1a"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("7023ad71-ae38-457d-a9ae-3e5c54fa0397"));
        }
    }
}
