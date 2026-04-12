using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Olimpo.ProductAPI.Migrations
{
    public partial class RenameMercadoPagoColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @old_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'orders'
      AND COLUMN_NAME = 'MercadoPagoPaymentId'
);

SET @new_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'orders'
      AND COLUMN_NAME = 'MercadoPagoId'
);

SET @sql := IF(
    @old_exists = 1 AND @new_exists = 0,
    'ALTER TABLE `orders` RENAME COLUMN `MercadoPagoPaymentId` TO `MercadoPagoId`;',
    'SELECT 1;'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
SET @old_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'orders'
      AND COLUMN_NAME = 'MercadoPagoId'
);

SET @new_exists := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'orders'
      AND COLUMN_NAME = 'MercadoPagoPaymentId'
);

SET @sql := IF(
    @old_exists = 1 AND @new_exists = 0,
    'ALTER TABLE `orders` RENAME COLUMN `MercadoPagoId` TO `MercadoPagoPaymentId`;',
    'SELECT 1;'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
");
        }
    }
}