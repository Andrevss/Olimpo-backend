using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olimpo.ProductAPI.Migrations
{
    public partial class RenameMercadoPagoColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MercadoPagoPaymentId",
                table: "orders",
                newName: "MercadoPagoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MercadoPagoId",
                table: "orders",
                newName: "MercadoPagoPaymentId");
        }
    }
}