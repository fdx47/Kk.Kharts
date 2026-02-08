using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedCompanyIdToVpnProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ovpn_file_content",
                schema: "kropkharts",
                table: "vpn_profiles",
                newName: "ovpn_content");

            migrationBuilder.AddColumn<int>(
                name: "assigned_company_id",
                schema: "kropkharts",
                table: "vpn_profiles",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("a2d35ef2-9d52-4eac-94a5-0db7561dd584"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("ada55a29-1d8b-4d87-8baf-93aded8c8309"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("31796fab-9f8d-4031-baa0-7ebff2890eff"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("ba323dcf-0c52-4199-b1c0-e3069882675d"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("4f76ea8e-a480-4dab-b2e1-e6f497857297"));

            migrationBuilder.CreateIndex(
                name: "IX_vpn_profiles_assigned_company_id",
                schema: "kropkharts",
                table: "vpn_profiles",
                column: "assigned_company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_vpn_profiles_companies_assigned_company_id",
                schema: "kropkharts",
                table: "vpn_profiles",
                column: "assigned_company_id",
                principalSchema: "kropkharts",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vpn_profiles_companies_assigned_company_id",
                schema: "kropkharts",
                table: "vpn_profiles");

            migrationBuilder.DropIndex(
                name: "IX_vpn_profiles_assigned_company_id",
                schema: "kropkharts",
                table: "vpn_profiles");

            migrationBuilder.DropColumn(
                name: "assigned_company_id",
                schema: "kropkharts",
                table: "vpn_profiles");

            migrationBuilder.RenameColumn(
                name: "ovpn_content",
                schema: "kropkharts",
                table: "vpn_profiles",
                newName: "ovpn_file_content");

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
        }
    }
}
