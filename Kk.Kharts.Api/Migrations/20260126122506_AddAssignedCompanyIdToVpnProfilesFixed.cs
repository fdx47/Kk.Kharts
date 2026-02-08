using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedCompanyIdToVpnProfilesFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vpn_profiles_companies_assigned_company_id",
                schema: "kropkharts",
                table: "vpn_profiles");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("8943d697-d37a-4e0c-94c0-73b4ae650b4e"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("f23a8b5f-7a39-4e8a-b74c-9a2e9896d514"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("9a5734f6-9a73-477b-a389-3d706d9294d6"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("3b4b69b8-c625-4b93-90eb-301a765d6c1e"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("953f5ad6-9fa6-4c99-8bbc-3c55d000008c"));

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

            migrationBuilder.AddForeignKey(
                name: "FK_vpn_profiles_companies_assigned_company_id",
                schema: "kropkharts",
                table: "vpn_profiles",
                column: "assigned_company_id",
                principalSchema: "kropkharts",
                principalTable: "companies",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
