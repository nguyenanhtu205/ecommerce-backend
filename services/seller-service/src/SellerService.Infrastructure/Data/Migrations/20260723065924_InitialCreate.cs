using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SellerService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shops",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    pickup_address_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "PendingSetup"),
                    is_linked_to_main_account = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shops", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_bank_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    account_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    account_holder = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_bank_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_shop_bank_accounts_shops_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_chat_quick_replies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_chat_quick_replies", x => x.id);
                    table.ForeignKey(
                        name: "FK_shop_chat_quick_replies_shops_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_chat_settings",
                columns: table => new
                {
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    auto_reply_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    auto_reply_message = table.Column<string>(type: "text", nullable: true),
                    away_mode_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_chat_settings", x => x.shop_id);
                    table.ForeignKey(
                        name: "FK_shop_chat_settings_shops_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_payment_settings",
                columns: table => new
                {
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payout_cycle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Weekly")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_payment_settings", x => x.shop_id);
                    table.ForeignKey(
                        name: "FK_shop_payment_settings_shops_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_shipping_carrier_connections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    carrier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    connected_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_shipping_carrier_connections", x => x.id);
                    table.ForeignKey(
                        name: "FK_shop_shipping_carrier_connections_shops_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_vacation_settings",
                columns: table => new
                {
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_vacation_settings", x => x.shop_id);
                    table.ForeignKey(
                        name: "FK_shop_vacation_settings_shops_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shop_bank_accounts_shop_id",
                table: "shop_bank_accounts",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_shop_chat_quick_replies_shop_id",
                table: "shop_chat_quick_replies",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_shop_shipping_carrier_connections_shop_id_carrier_id",
                table: "shop_shipping_carrier_connections",
                columns: new[] { "shop_id", "carrier_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shops_owner_user_id",
                table: "shops",
                column: "owner_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_bank_accounts");

            migrationBuilder.DropTable(
                name: "shop_chat_quick_replies");

            migrationBuilder.DropTable(
                name: "shop_chat_settings");

            migrationBuilder.DropTable(
                name: "shop_payment_settings");

            migrationBuilder.DropTable(
                name: "shop_shipping_carrier_connections");

            migrationBuilder.DropTable(
                name: "shop_vacation_settings");

            migrationBuilder.DropTable(
                name: "shops");
        }
    }
}
