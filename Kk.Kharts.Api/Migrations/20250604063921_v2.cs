using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen_at_time",
                schema: "kropkharts",
                table: "devices",
                type: "datetime2",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_seen_at",
                schema: "kropkharts",
                table: "devices",
                type: "datetime2",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 7L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 9L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 13L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 14L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 15L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 16L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 17L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 18L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "2017 - 10 - 25 00:00:00" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "last_seen_at_time",
                schema: "kropkharts",
                table: "devices",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "last_seen_at",
                schema: "kropkharts",
                table: "devices",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldMaxLength: 32);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 7L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 9L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 13L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 14L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 15L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 16L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "??" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 17L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 18L,
                columns: new[] { "last_seen_at", "last_seen_at_time", "last_send_at" },
                values: new object[] { "??", "??", "" });
        }
    }
}
