using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olimpo.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStockReservationSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservedStock",
                table: "products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "stock_reservations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ExpireAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsReleased = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_reservations_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_stock_reservations_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 20, 27, 38, 295, DateTimeKind.Utc).AddTicks(7808));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 20, 27, 38, 295, DateTimeKind.Utc).AddTicks(7810));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 20, 27, 38, 295, DateTimeKind.Utc).AddTicks(7811));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 20, 27, 38, 295, DateTimeKind.Utc).AddTicks(7934));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 3, 20, 27, 38, 295, DateTimeKind.Utc).AddTicks(7936));

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_ExpireAt",
                table: "stock_reservations",
                column: "ExpireAt");

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_IsReleased",
                table: "stock_reservations",
                column: "IsReleased");

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_OrderId_IsReleased",
                table: "stock_reservations",
                columns: new[] { "OrderId", "IsReleased" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_ProductId",
                table: "stock_reservations",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_reservations");

            migrationBuilder.DropColumn(
                name: "ReservedStock",
                table: "products");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 21, 6, 49, 529, DateTimeKind.Utc).AddTicks(1310));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 21, 6, 49, 529, DateTimeKind.Utc).AddTicks(1313));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 21, 6, 49, 529, DateTimeKind.Utc).AddTicks(1314));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 21, 6, 49, 529, DateTimeKind.Utc).AddTicks(1467));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 21, 6, 49, 529, DateTimeKind.Utc).AddTicks(1469));
        }
    }
}
