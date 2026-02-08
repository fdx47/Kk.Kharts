using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class v5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "kropkharts",
                table: "technicians",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_technicians_UserId",
                schema: "kropkharts",
                table: "technicians",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_technicians_users_UserId",
                schema: "kropkharts",
                table: "technicians",
                column: "UserId",
                principalSchema: "kropkharts",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_technicians_users_UserId",
                schema: "kropkharts",
                table: "technicians");

            migrationBuilder.DropIndex(
                name: "IX_technicians_UserId",
                schema: "kropkharts",
                table: "technicians");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "kropkharts",
                table: "technicians");
        }
    }
}
