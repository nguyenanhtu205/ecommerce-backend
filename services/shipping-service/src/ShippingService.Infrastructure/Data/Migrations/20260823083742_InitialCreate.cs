using System;
using Microsoft.EntityFrameworkCore.Migrations;
using ShippingService.Domain.Common;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShippingService.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carriers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carriers", x => x.id);
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
                name: "pickup_points",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    carrier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pickup_points", x => x.id);
                    table.ForeignKey(
                        name: "FK_pickup_points_carriers_carrier_id",
                        column: x => x.carrier_id,
                        principalTable: "carriers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shipments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    carrier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_code = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    pickup_address_snapshot = table.Column<AddressSnapshot>(type: "jsonb", nullable: false),
                    delivery_address_snapshot = table.Column<AddressSnapshot>(type: "jsonb", nullable: false),
                    estimated_delivery_start = table.Column<DateOnly>(type: "date", nullable: true),
                    estimated_delivery_end = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipments", x => x.id);
                    table.ForeignKey(
                        name: "FK_shipments_carriers_carrier_id",
                        column: x => x.carrier_id,
                        principalTable: "carriers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shipment_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    changed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipment_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_shipment_status_history_shipments_shipment_id",
                        column: x => x.shipment_id,
                        principalTable: "shipments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "carriers",
                columns: new[] { "id", "code", "name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "mock", "Giao Hàng Thử Nghiệm" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "ghn", "Giao Hàng Nhanh" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "ghtk", "Giao Hàng Tiết Kiệm" }
                });

            migrationBuilder.InsertData(
                table: "pickup_points",
                columns: new[] { "id", "address", "carrier_id", "name" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "123 Nguyễn Trãi, Thanh Xuân, Hà Nội", new Guid("11111111-1111-1111-1111-111111111111"), "Điểm lấy hàng ở Hà Nội" },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "456 Nguyễn Thị Minh Khai, Quận 3, TP. Hồ Chí Minh", new Guid("11111111-1111-1111-1111-111111111111"), "Điểm lấy hàng ở Hồ Chí Minh" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "789 Nguyễn Văn Linh, Hải Châu, Đà Nẵng", new Guid("11111111-1111-1111-1111-111111111111"), "Điểm lấy hàng ở Đà Nẵng" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_carriers_code",
                table: "carriers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_processed_at_created_at",
                table: "outbox_messages",
                columns: new[] { "processed_at", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_pickup_points_carrier_id",
                table: "pickup_points",
                column: "carrier_id");

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_event_id",
                table: "processed_events",
                column: "event_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_event_type",
                table: "processed_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_shipment_status_history_shipment_id",
                table: "shipment_status_history",
                column: "shipment_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipments_carrier_id",
                table: "shipments",
                column: "carrier_id");

            migrationBuilder.CreateIndex(
                name: "IX_shipments_order_id",
                table: "shipments",
                column: "order_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "pickup_points");

            migrationBuilder.DropTable(
                name: "processed_events");

            migrationBuilder.DropTable(
                name: "shipment_status_history");

            migrationBuilder.DropTable(
                name: "shipments");

            migrationBuilder.DropTable(
                name: "carriers");
        }
    }
}
