using System;
using Microsoft.EntityFrameworkCore.Migrations;
using OrderService.Domain.Common;

#nullable disable

namespace OrderService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "checkout_saga_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_ids = table.Column<string>(type: "jsonb", nullable: false),
                    reserved_order_ids = table.Column<string>(type: "jsonb", nullable: false),
                    total_amount = table.Column<int>(type: "integer", nullable: false),
                    order_shares = table.Column<string>(type: "jsonb", nullable: false),
                    payment_method = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    platform_voucher_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    shop_vouchers = table.Column<string>(type: "jsonb", nullable: false),
                    voucher_redeemed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    redirect_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    fail_reason = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checkout_saga_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_reservation_saga_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    checkout_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    carrier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pickup_address_snapshot = table.Column<AddressSnapshot>(type: "jsonb", nullable: true),
                    delivery_address_snapshot = table.Column<AddressSnapshot>(type: "jsonb", nullable: true),
                    items = table.Column<string>(type: "jsonb", nullable: false),
                    fail_reason = table.Column<string>(type: "text", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_reservation_saga_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_name = table.Column<string>(type: "text", nullable: false),
                    checkout_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    merchandise_subtotal = table.Column<int>(type: "integer", nullable: false),
                    shipping_fee = table.Column<int>(type: "integer", nullable: false),
                    voucher_discount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    xu_discount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_payment = table.Column<int>(type: "integer", nullable: false),
                    shipping_address_snapshot = table.Column<AddressSnapshot>(type: "jsonb", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processed_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    event_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    combination_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "text", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: false),
                    variation = table.Column<string>(type: "text", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    original_price = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_shipping_snapshots",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    carrier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    carrier_name = table.Column<string>(type: "text", nullable: false),
                    fee = table.Column<int>(type: "integer", nullable: false),
                    estimated_delivery_start = table.Column<DateOnly>(type: "date", nullable: true),
                    estimated_delivery_end = table.Column<DateOnly>(type: "date", nullable: true),
                    late_delivery_compensation = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_shipping_snapshots", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_order_shipping_snapshots_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    changed_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_status_history_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_vouchers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    voucher_code = table.Column<string>(type: "text", nullable: true),
                    discount_amount = table.Column<int>(type: "integer", nullable: false),
                    scope = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_vouchers", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_vouchers_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item_addons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item_addons", x => x.id);
                    table.ForeignKey(
                        name: "FK_order_item_addons_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_checkout_saga_states_buyer_id",
                table: "checkout_saga_states",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_addons_order_item_id",
                table: "order_item_addons",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_reservation_saga_states_checkout_batch_id",
                table: "order_reservation_saga_states",
                column: "checkout_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_status_history_order_id",
                table: "order_status_history",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_vouchers_order_id",
                table: "order_vouchers",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_buyer_id",
                table: "orders",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_buyer_id_status",
                table: "orders",
                columns: new[] { "buyer_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_checkout_batch_id",
                table: "orders",
                column: "checkout_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_shop_id",
                table: "orders",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_processed_at_created_at",
                table: "outbox_messages",
                columns: new[] { "processed_at", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_event_id",
                table: "processed_events",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_event_type",
                table: "processed_events",
                column: "event_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checkout_saga_states");

            migrationBuilder.DropTable(
                name: "order_item_addons");

            migrationBuilder.DropTable(
                name: "order_reservation_saga_states");

            migrationBuilder.DropTable(
                name: "order_shipping_snapshots");

            migrationBuilder.DropTable(
                name: "order_status_history");

            migrationBuilder.DropTable(
                name: "order_vouchers");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "processed_events");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "orders");
        }
    }
}
