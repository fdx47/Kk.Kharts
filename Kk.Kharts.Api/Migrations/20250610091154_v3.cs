using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_seen_at_time",
                schema: "kropkharts",
                table: "devices");

            migrationBuilder.AddColumn<DateTime>(
                name: "InstallationDate",
                schema: "kropkharts",
                table: "devices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Seller",
                schema: "kropkharts",
                table: "devices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PendingEmailChanges",
                schema: "kropkharts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NewEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Confirmed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingEmailChanges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingPasswordResets",
                schema: "kropkharts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingPasswordResets", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 5L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 6L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 7L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 8L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 9L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 10L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 11L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 12L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 13L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 14L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 15L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 16L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 10, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 17L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2024, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 18L,
                columns: new[] { "InstallationDate", "Seller" },
                values: new object[] { new DateTime(2025, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "3CTEC" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingEmailChanges",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "PendingPasswordResets",
                schema: "kropkharts");

            migrationBuilder.DropColumn(
                name: "InstallationDate",
                schema: "kropkharts",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "Seller",
                schema: "kropkharts",
                table: "devices");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_seen_at_time",
                schema: "kropkharts",
                table: "devices",
                type: "datetime2",
                maxLength: 40,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 1L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 2L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 3L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 4L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 5L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 6L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 7L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 8L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 9L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 10L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 11L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 12L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 13L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 14L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 15L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 16L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 17L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "devices",
                keyColumn: "Id",
                keyValue: 18L,
                column: "last_seen_at_time",
                value: new DateTime(2017, 10, 25, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
