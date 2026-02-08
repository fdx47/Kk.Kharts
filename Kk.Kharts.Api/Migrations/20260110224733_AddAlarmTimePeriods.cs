using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAlarmTimePeriods : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseTimePeriods",
                schema: "kropkharts",
                table: "alarm_rules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "alarm_time_periods",
                schema: "kropkharts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alarm_rule_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    start_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "time", nullable: false),
                    low_value = table.Column<float>(type: "real", nullable: true),
                    high_value = table.Column<float>(type: "real", nullable: true),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alarm_time_periods", x => x.id);
                    table.ForeignKey(
                        name: "FK_alarm_time_periods_alarm_rules_alarm_rule_id",
                        column: x => x.alarm_rule_id,
                        principalSchema: "kropkharts",
                        principalTable: "alarm_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("67a53df4-672c-4005-b099-da71ed1416ca"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("44a660e9-7e22-41f2-b6a7-f037eca4afdb"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("ee4941cb-73b6-45aa-938b-d1c7900bc56d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("7be98d8b-4a15-4f97-ac44-a08e18f0e6f7"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("ef1060ef-335b-402f-874b-7b740130155e"));

            migrationBuilder.CreateIndex(
                name: "IX_alarm_time_periods_alarm_rule_id_display_order",
                schema: "kropkharts",
                table: "alarm_time_periods",
                columns: new[] { "alarm_rule_id", "display_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alarm_time_periods",
                schema: "kropkharts");

            migrationBuilder.DropColumn(
                name: "UseTimePeriods",
                schema: "kropkharts",
                table: "alarm_rules");

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
        }
    }
}
