using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kropkharts");

            migrationBuilder.CreateTable(
                name: "companies",
                schema: "kropkharts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    parent_company_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.id);
                    table.ForeignKey(
                        name: "FK_companies_companies_parent_company_id",
                        column: x => x.parent_company_id,
                        principalSchema: "kropkharts",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "device_model",
                schema: "kropkharts",
                columns: table => new
                {
                    ModelId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    model = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_model", x => x.ModelId);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "kropkharts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    company_id = table.Column<int>(type: "int", nullable: false),
                    access_level = table.Column<int>(type: "int", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    header_name = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "3ctec"),
                    header_value = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "demo"),
                    last_user_agent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    last_ip_address = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    refresh_token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    refresh_token_expiry_time = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    signup_date = table.Column<DateTime>(name: "signup_date)\r\n", type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_companies_company_id",
                        column: x => x.company_id,
                        principalSchema: "kropkharts",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "kropkharts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dev_eui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    name = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    description = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    societe_id = table.Column<int>(type: "int", nullable: true),
                    battery = table.Column<float>(type: "real", nullable: false),
                    last_send_at = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    last_seen_at = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    last_seen_at_time = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    model_id = table.Column<long>(type: "bigint", nullable: true),
                    active_in_kropkontrol = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_devices_companies_societe_id",
                        column: x => x.societe_id,
                        principalSchema: "kropkharts",
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_devices_devices_models_DeviceModelId",
                        column: x => x.model_id,
                        principalSchema: "kropkharts",
                        principalTable: "device_model",
                        principalColumn: "ModelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dashboards",
                schema: "kropkharts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dashboards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dashboards_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "kropkharts",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "em300_th",
                schema: "kropkharts",
                columns: table => new
                {
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dev_eui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    device_id = table.Column<long>(type: "bigint", nullable: false),
                    temperature = table.Column<float>(type: "real", nullable: false),
                    humidite = table.Column<float>(type: "real", nullable: false),
                    batterie = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_em300_th", x => new { x.timestamp, x.dev_eui });
                    table.ForeignKey(
                        name: "FK_em300_th_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "kropkharts",
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "uc502_modbus",
                schema: "kropkharts",
                columns: table => new
                {
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dev_eui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    device_id = table.Column<long>(type: "bigint", nullable: false),
                    modbus_chn_1 = table.Column<float>(type: "real", nullable: true),
                    modbus_chn_2 = table.Column<float>(type: "real", nullable: true),
                    modbus_chn_3 = table.Column<float>(type: "real", nullable: true),
                    modbus_chn_4 = table.Column<float>(type: "real", nullable: true),
                    modbus_chn_5 = table.Column<float>(type: "real", nullable: true),
                    modbus_chn_6 = table.Column<float>(type: "real", nullable: true),
                    batterie = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uc502_modbus", x => new { x.timestamp, x.dev_eui });
                    table.ForeignKey(
                        name: "FK_uc502_modbus_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "kropkharts",
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wets_150",
                schema: "kropkharts",
                columns: table => new
                {
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dev_eui = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    device_id = table.Column<long>(type: "bigint", nullable: false),
                    permittivite = table.Column<float>(type: "real", nullable: false),
                    ec_bulk = table.Column<float>(type: "real", nullable: false),
                    soil_temperature = table.Column<float>(type: "real", nullable: false),
                    mineral_vwc = table.Column<float>(type: "real", nullable: false),
                    mineral_ecp = table.Column<float>(type: "real", nullable: false),
                    organic_vwc = table.Column<float>(type: "real", nullable: false),
                    organic_ecp = table.Column<float>(type: "real", nullable: false),
                    peatmix_vwc = table.Column<float>(type: "real", nullable: false),
                    peatmix_ecp = table.Column<float>(type: "real", nullable: false),
                    coir_vwc = table.Column<float>(type: "real", nullable: false),
                    coir_ecp = table.Column<float>(type: "real", nullable: false),
                    minwool_vwc = table.Column<float>(type: "real", nullable: false),
                    minwool_ecp = table.Column<float>(type: "real", nullable: false),
                    perlite_vwc = table.Column<float>(type: "real", nullable: false),
                    perlite_ecp = table.Column<float>(type: "real", nullable: false),
                    batterie = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wets_150", x => new { x.timestamp, x.dev_eui });
                    table.ForeignKey(
                        name: "FK_wets_150_devices_device_id",
                        column: x => x.device_id,
                        principalSchema: "kropkharts",
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "kropkharts",
                table: "companies",
                columns: new[] { "id", "name", "parent_company_id" },
                values: new object[,]
                {
                    { 1, "3CTEC", null },
                    { 2, "Stratberries", null },
                    { 3, "Invenio", null },
                    { 4, "Pozzobon", null },
                    { 5, "Baudas", null }
                });

            migrationBuilder.InsertData(
                schema: "kropkharts",
                table: "device_model",
                columns: new[] { "ModelId", "description", "model" },
                values: new object[,]
                {
                    { 1L, "xxxxxxx", "EM300-CL" },
                    { 2L, "xxxxxxx", "EM300-DI" },
                    { 3L, "xxxxxxx", "EM300-DI_Hall" },
                    { 4L, "xxxxxxx", "EM300-MCS" },
                    { 5L, "xxxxxxx", "EM300-MLD" },
                    { 6L, "xxxxxxx", "EM300-SLD" },
                    { 7L, "Temperature & Humidity Sensor", "EM300-TH" },
                    { 8L, "xxxxxxx", "EM310-TILT" },
                    { 9L, "xxxxxxx", "EM310-UDL" },
                    { 10L, "xxxxxxx", "EM320-TH" },
                    { 11L, "xxxxxxx", "EM320-TILT" },
                    { 12L, "Reserved", "EM300-Reserved9" },
                    { 13L, "Reserved", "EM300-Reserved8" },
                    { 14L, "Reserved", "EM300-Reserved7" },
                    { 15L, "Reserved", "EM300-Reserved6" },
                    { 16L, "Reserved", "EM300-Reserved5" },
                    { 17L, "Reserved", "EM300-Reserved4" },
                    { 18L, "Reserved", "EM300-Reserved3" },
                    { 19L, "Reserved", "EM300-Reserved2" },
                    { 20L, "Reserved", "EM300-Reserved1" },
                    { 21L, "xxxxxxx", "EM500-CO2" },
                    { 22L, "xxxxxxx", "EM500-LGT" },
                    { 23L, "xxxxxxx", "EM500-PP" },
                    { 24L, "xxxxxxx", "EM500-PT100" },
                    { 25L, "Soil Moisture, Temperature and Electrical Conductivity Sensor", "EM500-SMTC" },
                    { 26L, "xxxxxxx", "EM500-SWL" },
                    { 27L, "xxxxxxx", "EM500-UDL" },
                    { 28L, "Reserved", "EM500-Reserved13" },
                    { 29L, "Reserved", "EM500-Reserved12" },
                    { 30L, "Reserved", "EM500-Reserved11" },
                    { 31L, "Reserved", "EM500-Reserved10" },
                    { 32L, "Reserved", "EM500-Reserved9" },
                    { 33L, "Reserved", "EM500-Reserved8" },
                    { 34L, "Reserved", "EM500-Reserved7" },
                    { 35L, "Reserved", "EM500-Reserved6" },
                    { 36L, "Reserved", "EM500-Reserved5" },
                    { 37L, "Reserved", "EM500-Reserved4" },
                    { 38L, "Reserved", "EM500-Reserved3" },
                    { 39L, "Reserved", "EM500-Reserved2" },
                    { 40L, "Reserved", "EM500-Reserved1" },
                    { 41L, "xxxxxxx", "UC100" },
                    { 42L, "xxxxxxx", "UC11-N1" },
                    { 43L, "xxxxxxx", "UC11-T1" },
                    { 44L, "xxxxxxx", "UC11XX" },
                    { 45L, "UC300 Iot Controller", "UC300" },
                    { 46L, "xxxxxxx", "UC3XXX" },
                    { 47L, "Multi-Interface Controller", "UC502" },
                    { 48L, "Reserved", "UC51X" },
                    { 49L, "Reserved", "UC-Series-Reserved12" },
                    { 50L, "Reserved", "UC-Series-Reserved11" },
                    { 51L, "Reserved", "UC-Series-Reserved10" },
                    { 52L, "Reserved", "UC-Series-Reserved9" },
                    { 53L, "Reserved", "UC-Series-Reserved8" },
                    { 54L, "Reserved", "UC-Series-Reserved7" },
                    { 55L, "Reserved", "UC-Series-Reserved6" },
                    { 56L, "Reserved", "UC-Series-Reserved5" },
                    { 57L, "Reserved", "UC-Series-Reserved4" },
                    { 58L, "Reserved", "UC-Series-Reserved3" },
                    { 59L, "Reserved", "UC-Series-Reserved2" },
                    { 60L, "Reserved", "UC-Series-Reserved1" },
                    { 61L, "Modbus", "U502" }
                });

            migrationBuilder.InsertData(
                schema: "kropkharts",
                table: "devices",
                columns: new[] { "Id", "active_in_kropkontrol", "battery", "description", "dev_eui", "last_seen_at", "last_seen_at_time", "last_send_at", "model_id", "name", "societe_id" },
                values: new object[,]
                {
                    { 1L, true, 69.69f, "WET150 - 001", "24E124454E353385", "??", "??", "??", 47L, "UC502_001", 3 },
                    { 2L, true, 69.69f, "WET150 - 002", "24E124454E353580", "??", "??", "??", 47L, "UC502_002", 3 },
                    { 3L, true, 69.69f, "WET150 - 003", "24E124454E353875", "??", "??", "??", 47L, "UC502_003", 3 },
                    { 4L, true, 69.69f, "WET150 - 004", "24E124454E042988", "??", "??", "??", 47L, "UC502_004", 3 },
                    { 5L, true, 69.69f, "WET150 - 005", "24E124454E353844", "??", "??", "??", 47L, "UC502_005", 3 },
                    { 6L, true, 69.69f, "WET150 - 006", "24E124454E353679", "??", "??", "??", 47L, "UC502_006", 3 },
                    { 7L, true, 69.69f, "WET150 - 007", "24E124454E048465", "??", "??", "??", 47L, "UC502_007", 3 },
                    { 8L, true, 69.69f, "WET150 - 008", "24E124454E353717", "??", "??", "??", 47L, "UC502_008", 3 },
                    { 9L, true, 69.69f, "WET150 - 009", "24E124454E353777", "??", "??", "??", 47L, "UC502_009", 3 },
                    { 10L, true, 69.69f, "Invenio Tomate", "24E124454E352976", "??", "??", "??", 47L, "UC502_010", 3 },
                    { 11L, true, 69.69f, "Temp & Hr - 001", "24E124136E318864", "??", "??", "??", 7L, "EM300_TH_001", 3 },
                    { 12L, true, 69.69f, "Temp & Hr - 002", "24E124136E317494", "??", "??", "??", 7L, "EM300_TH_002", 3 },
                    { 13L, true, 69.69f, "Temp & Hr - 003", "24E124136E317560", "??", "??", "??", 7L, "EM300_TH_003", 3 },
                    { 14L, true, 69.69f, "Temp & Hr - 004", "24E124136E311302", "??", "??", "??", 7L, "EM300_TH_004", 3 },
                    { 15L, true, 69.69f, "Temp & Hr - 005", "24E124136E316874", "??", "??", "??", 7L, "EM300_TH_005", 3 },
                    { 16L, true, 69.69f, "Temp & Hr - 006", "24E124136E311882", "??", "??", "??", 7L, "EM300_TH_006", 3 },
                    { 17L, true, 69.69f, "Demo Pozzobon Modbus - Solarimètre", "24E124454E045483", "??", "??", "", 61L, "UC502_045483", 2 },
                    { 18L, true, 69.69f, "Demo Pozzobon - Wet150", "24E124454E353793", "??", "??", "", 47L, "UC502_353793", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_companies_parent_company_id",
                schema: "kropkharts",
                table: "companies",
                column: "parent_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_dashboards_UserId",
                schema: "kropkharts",
                table: "dashboards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_devices_model_id",
                schema: "kropkharts",
                table: "devices",
                column: "model_id");

            migrationBuilder.CreateIndex(
                name: "IX_devices_societe_id",
                schema: "kropkharts",
                table: "devices",
                column: "societe_id");

            migrationBuilder.CreateIndex(
                name: "IX_em300_th_device_id",
                schema: "kropkharts",
                table: "em300_th",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_uc502_modbus_device_id",
                schema: "kropkharts",
                table: "uc502_modbus",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_company_id",
                schema: "kropkharts",
                table: "users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_wets_150_device_id",
                schema: "kropkharts",
                table: "wets_150",
                column: "device_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dashboards",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "em300_th",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "uc502_modbus",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "wets_150",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "users",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "companies",
                schema: "kropkharts");

            migrationBuilder.DropTable(
                name: "device_model",
                schema: "kropkharts");
        }
    }
}
