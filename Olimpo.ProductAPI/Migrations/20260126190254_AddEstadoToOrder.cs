using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olimpo.ProductAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 2, 53, 759, DateTimeKind.Utc).AddTicks(7270));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 2, 53, 759, DateTimeKind.Utc).AddTicks(7272));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 2, 53, 759, DateTimeKind.Utc).AddTicks(7273));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 2, 53, 759, DateTimeKind.Utc).AddTicks(7387));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 19, 2, 53, 759, DateTimeKind.Utc).AddTicks(7389));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 54, 42, 867, DateTimeKind.Utc).AddTicks(3940));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 54, 42, 867, DateTimeKind.Utc).AddTicks(3942));

            migrationBuilder.UpdateData(
                table: "categories",
                keyColumn: "Id",
                keyValue: 3L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 54, 42, 867, DateTimeKind.Utc).AddTicks(3943));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 1L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 54, 42, 867, DateTimeKind.Utc).AddTicks(4084));

            migrationBuilder.UpdateData(
                table: "products",
                keyColumn: "Id",
                keyValue: 2L,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 26, 18, 54, 42, 867, DateTimeKind.Utc).AddTicks(4086));
        }
    }
}
