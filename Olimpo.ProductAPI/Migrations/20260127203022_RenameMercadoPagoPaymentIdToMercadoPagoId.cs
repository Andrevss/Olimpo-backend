using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olimpo.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameMercadoPagoPaymentIdToMercadoPagoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 20, 30, 21, 230, DateTimeKind.Utc).AddTicks(3559));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 20, 30, 21, 230, DateTimeKind.Utc).AddTicks(3561));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 20, 30, 21, 230, DateTimeKind.Utc).AddTicks(3563));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 20, 30, 21, 230, DateTimeKind.Utc).AddTicks(3720));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 20, 30, 21, 230, DateTimeKind.Utc).AddTicks(3722));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 43, 40, 411, DateTimeKind.Utc).AddTicks(2725));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 43, 40, 411, DateTimeKind.Utc).AddTicks(2728));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 43, 40, 411, DateTimeKind.Utc).AddTicks(2729));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 43, 40, 411, DateTimeKind.Utc).AddTicks(2874));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 27, 18, 43, 40, 411, DateTimeKind.Utc).AddTicks(2876));
        }
    }
}
