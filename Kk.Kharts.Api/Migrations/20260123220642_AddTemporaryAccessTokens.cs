using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kk.Kharts.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTemporaryAccessTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "temporary_access_tokens",
                schema: "kropkharts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    token_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    token_prefix = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    expires_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    issued_by_user_id = table.Column<int>(type: "int", nullable: true),
                    issued_by_email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    issued_by_telegram_user_id = table.Column<long>(type: "bigint", nullable: true),
                    usage_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    last_used_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    consumed_by_user_id = table.Column<int>(type: "int", nullable: true),
                    consumed_by_email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    revoked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temporary_access_tokens", x => x.id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_temporary_access_tokens_expiration",
                schema: "kropkharts",
                table: "temporary_access_tokens",
                column: "expires_at_utc");

            migrationBuilder.CreateIndex(
                name: "IX_temporary_access_tokens_prefix",
                schema: "kropkharts",
                table: "temporary_access_tokens",
                column: "token_prefix");

            migrationBuilder.CreateIndex(
                name: "UX_temporary_access_tokens_hash",
                schema: "kropkharts",
                table: "temporary_access_tokens",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "temporary_access_tokens",
                schema: "kropkharts");

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 1,
                column: "DeviceId",
                value: new Guid("bce51847-45ed-4958-a961-f1d76c6d14ee"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 2,
                column: "DeviceId",
                value: new Guid("892d9565-761a-4928-b24f-697a55d7ac8e"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 3,
                column: "DeviceId",
                value: new Guid("427ac466-790b-45e6-95e7-720f147c400c"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 4,
                column: "DeviceId",
                value: new Guid("10b5b855-ab88-442f-a240-276ab4012de5"));

            migrationBuilder.UpdateData(
                schema: "kropkharts",
                table: "companies",
                keyColumn: "id",
                keyValue: 5,
                column: "DeviceId",
                value: new Guid("1b3ed00d-f406-4b83-b9a7-308387862751"));
        }
    }
}
