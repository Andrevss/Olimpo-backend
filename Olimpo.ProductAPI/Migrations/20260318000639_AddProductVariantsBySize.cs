using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Olimpo.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariantsBySize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ProductVariantId",
                table: "stock_reservations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProductVariantId",
                table: "order_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "order_items",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_variants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Size = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Stock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReservedStock = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_variants_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 18, 0, 6, 38, 468, DateTimeKind.Utc).AddTicks(6708));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 18, 0, 6, 38, 468, DateTimeKind.Utc).AddTicks(6710));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 18, 0, 6, 38, 468, DateTimeKind.Utc).AddTicks(6712));

            migrationBuilder.InsertData(
                table: "product_variants",
                columns: new[] { "Id", "CreatedAt", "ProductId", "Size", "Stock", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, new DateTime(2026, 3, 18, 0, 6, 38, 468, DateTimeKind.Utc).AddTicks(6880), 2L, "P", 10, null },
                    { 2L, new DateTime(2026, 3, 18, 0, 6, 38, 468, DateTimeKind.Utc).AddTicks(6881), 2L, "M", 15, null },
                    { 3L, new DateTime(2026, 3, 18, 0, 6, 38, 468, DateTimeKind.Utc).AddTicks(6882), 2L, "G", 12, null }
                });

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 18, 0, 6, 38, 468, DateTimeKind.Utc).AddTicks(6853));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 18, 0, 6, 38, 468, DateTimeKind.Utc).AddTicks(6855));

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_ProductVariantId",
                table: "stock_reservations",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_ProductVariantId",
                table: "order_items",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId_Size",
                table: "product_variants",
                columns: new[] { "ProductId", "Size" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_order_items_product_variants_ProductVariantId",
                table: "order_items",
                column: "ProductVariantId",
                principalTable: "product_variants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_reservations_product_variants_ProductVariantId",
                table: "stock_reservations",
                column: "ProductVariantId",
                principalTable: "product_variants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_order_items_product_variants_ProductVariantId",
                table: "order_items");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_reservations_product_variants_ProductVariantId",
                table: "stock_reservations");

            migrationBuilder.DropTable(
                name: "product_variants");

            migrationBuilder.DropIndex(
                name: "IX_stock_reservations_ProductVariantId",
                table: "stock_reservations");

            migrationBuilder.DropIndex(
                name: "IX_order_items_ProductVariantId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "stock_reservations");

            migrationBuilder.DropColumn(
                name: "ProductVariantId",
                table: "order_items");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "order_items");

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
        }
    }
}
