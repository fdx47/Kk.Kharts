using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVpnProfilesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vpn_profiles",
                schema: "kropkharts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    common_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    vpn_ip = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ovpn_file_content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ovpn_file_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    assigned_user_id = table.Column<int>(type: "int", nullable: true),
                    installation_location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vpn_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_vpn_profiles_users_assigned_user_id",
                        column: x => x.assigned_user_id,
                        principalSchema: "kropkharts",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("2e972ae8-aeea-48c0-8bb1-a99c7a03c944"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("336efd56-c232-49b2-9858-42e577e17867"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("447f4fb3-9f80-4997-b4c7-94d7b4085ea6"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("a1115544-59e1-46d1-a3d9-2a7e268e52ec"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("435e088e-bf93-4193-953f-102736d76ac3"));

            migrationBuilder.CreateIndex(
                name: "IX_vpn_profiles_assigned_user_id",
                schema: "kropkharts",
                table: "vpn_profiles",
                column: "assigned_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_vpn_profiles_common_name",
                schema: "kropkharts",
                table: "vpn_profiles",
                column: "common_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vpn_profiles_vpn_ip",
                schema: "kropkharts",
                table: "vpn_profiles",
                column: "vpn_ip",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vpn_profiles",
                schema: "kropkharts");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("98fcb214-ae6d-4ed3-833f-8659152b7dc3"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("62ba794a-b755-4160-a9a2-7e2309a1b0a3"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("2f9f4522-9e4e-49ed-af13-b8858748f46b"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("68d42acd-fadc-4758-acef-e299dcdf9453"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("45f3d5d3-badf-4057-9e93-2ccc0fa1cd8a"));
        }
    }
}
