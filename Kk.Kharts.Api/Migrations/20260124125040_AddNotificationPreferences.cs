using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "notification_preference",
                schema: "kropkharts",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 1)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.CreateTable(
                name: "user_pushover_settings",
                schema: "kropkharts",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    app_token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    user_key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    sound = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    device = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    message_template = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    priority = table.Column<int>(type: "int", nullable: true),
                    retry_seconds = table.Column<int>(type: "int", nullable: true),
                    expire_seconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_pushover_settings", x => x.user_id);
                    table.ForeignKey(
                        name: "FK_user_pushover_settings_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "kropkharts",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("46226be1-9bda-4489-bbc4-7b17d9124ae4"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("b42a7475-8da1-4e51-8dd9-c61c30c4fbb4"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("bef982eb-a4b2-43d3-998e-2803370b6436"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("e3bc682b-c9ce-43a3-a6f8-15dc278551fe"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("de35bbca-d0f6-4ae5-b367-d69124026d5f"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_pushover_settings",
                schema: "kropkharts");

            migrationBuilder.DropColumn(
                name: "notification_preference",
                schema: "kropkharts",
                table: "users");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("b640f78b-f358-4ab8-b362-552f9f1c4d5f"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("5b331d39-e1b1-4e6a-a186-4963b112d09b"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("0082e8e6-4ce8-4001-9ece-d466c8e61ef4"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("29e6b5bc-1826-4c9e-824d-992c83cc711e"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("be417794-b673-4954-b9d8-1ff9ae8af144"));
        }
    }
}
