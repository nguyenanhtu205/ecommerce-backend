using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checkout_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    idempotency_key = table.Column<string>(type: "text", nullable: false),
                    provider_transaction_id = table.Column<string>(type: "text", nullable: true),
                    redirect_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_wallets",
                columns: table => new
                {
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    available_balance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    pending_balance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    debt_balance = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_wallets", x => x.shop_id);
                });

            migrationBuilder.CreateTable(
                name: "escrow_holds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Held"),
                    held_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    release_due_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    released_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_escrow_holds", x => x.id);
                    table.ForeignKey(
                        name: "FK_escrow_holds_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_order_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_order_links", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_order_links_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    idempotency_key = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.id);
                    table.ForeignKey(
                        name: "FK_refunds_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_wallet_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    escrow_hold_id = table.Column<Guid>(type: "uuid", nullable: true),
                    refund_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    available_balance_after = table.Column<int>(type: "integer", nullable: false),
                    debt_balance_after = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    EscrowHoldId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    RefundId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_wallet_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_shop_wallet_transactions_escrow_holds_EscrowHoldId1",
                        column: x => x.EscrowHoldId1,
                        principalTable: "escrow_holds",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_shop_wallet_transactions_escrow_holds_escrow_hold_id",
                        column: x => x.escrow_hold_id,
                        principalTable: "escrow_holds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shop_wallet_transactions_refunds_RefundId1",
                        column: x => x.RefundId1,
                        principalTable: "refunds",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_shop_wallet_transactions_refunds_refund_id",
                        column: x => x.refund_id,
                        principalTable: "refunds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_shop_wallet_transactions_shop_wallets_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop_wallets",
                        principalColumn: "shop_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_escrow_holds_order_id",
                table: "escrow_holds",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_escrow_holds_payment_id",
                table: "escrow_holds",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_escrow_holds_shop_id",
                table: "escrow_holds",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_escrow_holds_status_release_due_at",
                table: "escrow_holds",
                columns: new[] { "status", "release_due_at" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_links_order_id",
                table: "payment_order_links",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_order_links_payment_id",
                table: "payment_order_links",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_buyer_id",
                table: "payments",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_checkout_batch_id",
                table: "payments",
                column: "checkout_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_idempotency_key",
                table: "payments",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refunds_idempotency_key",
                table: "refunds",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refunds_order_id",
                table: "refunds",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_refunds_payment_id",
                table: "refunds",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_shop_wallet_transactions_escrow_hold_id",
                table: "shop_wallet_transactions",
                column: "escrow_hold_id");

            migrationBuilder.CreateIndex(
                name: "IX_shop_wallet_transactions_EscrowHoldId1",
                table: "shop_wallet_transactions",
                column: "EscrowHoldId1");

            migrationBuilder.CreateIndex(
                name: "IX_shop_wallet_transactions_order_id",
                table: "shop_wallet_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_shop_wallet_transactions_refund_id",
                table: "shop_wallet_transactions",
                column: "refund_id");

            migrationBuilder.CreateIndex(
                name: "IX_shop_wallet_transactions_RefundId1",
                table: "shop_wallet_transactions",
                column: "RefundId1");

            migrationBuilder.CreateIndex(
                name: "IX_shop_wallet_transactions_shop_id",
                table: "shop_wallet_transactions",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "IX_shop_wallet_transactions_shop_id_created_at",
                table: "shop_wallet_transactions",
                columns: new[] { "shop_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_order_links");

            migrationBuilder.DropTable(
                name: "shop_wallet_transactions");

            migrationBuilder.DropTable(
                name: "escrow_holds");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "shop_wallets");

            migrationBuilder.DropTable(
                name: "payments");
        }
    }
}
