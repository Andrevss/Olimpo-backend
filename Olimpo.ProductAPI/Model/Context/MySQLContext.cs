using Microsoft.EntityFrameworkCore;
using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Model.Context
{
    public class MySQLContext : DbContext
    {
        public MySQLContext(DbContextOptions<MySQLContext> options) : base(options) { }

        // DbSets - Representam as tabelas no banco
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<StockReservation> StockReservation { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // CONFIGURAÇÃO DA TABELA CATEGORY
            // ============================================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);

                // Índice para busca por nome
                entity.HasIndex(e => e.Name);
            });

            // ============================================
            // CONFIGURAÇÃO DA TABELA PRODUCT
            // ============================================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("products");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)"); // Ex: 99999999.99

                entity.Property(e => e.Stock)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);

                entity.Property(e => e.ReservedStock)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Ignore(e => e.AvailableStock); // Propriedade calculada, não mapeada

                // Relacionamento: Product -> Category
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict); // Não permite deletar categoria com produtos

                // Índices para performance
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.CategoryId);
            });

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("product_variants");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Size)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Stock)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.ReservedStock)
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt);

                entity.Ignore(e => e.AvailableStock);

                entity.HasOne(v => v.Product)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(v => v.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ProductId, e.Size }).IsUnique();
            });

            // ============================================
            // CONFIGURAÇÃO DA TABELA ORDER
            // ============================================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.NomeCompleto)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Telefone)
                    .IsRequired()
                    .HasMaxLength(15);
                entity.Property(e => e.Rua)
                    .IsRequired()
                    .HasMaxLength(150);
                entity.Property(e => e.Numero)
                    .IsRequired()
                    .HasMaxLength(10);
                entity.Property(e => e.Bairro)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Cidade)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.Estado)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Cep)
                    .IsRequired()
                    .HasMaxLength(20);
                entity.Property(e => e.Complemento)
                    .HasMaxLength(250);

                entity.Property(e => e.Total)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status)
                    .IsRequired();
                entity.Property(e => e.MercadoPagoId)
                    .HasMaxLength(100);
                entity.Property(e => e.MercadoPagoPaymentStatus)
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.MercadoPagoId);
            });

            // ============================================
            // CONFIGURAÇÃO DA TABELA ORDERITEM
            // ============================================

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(10,2)");

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(oi => oi.ProductVariant)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.ProductVariantId);
            });

            // ============================================
            // CONFIGURAÇÃO DA TABELA STOCK_RESERVATION
            // ============================================
            modelBuilder.Entity<StockReservation>(entity =>
            {
                entity.ToTable("stock_reservations");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.ExpireAt).IsRequired();
                entity.Property(e => e.IsReleased).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(sr => sr.Product)
                    .WithMany()
                    .HasForeignKey(sr => sr.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.ProductVariant)
                    .WithMany()
                    .HasForeignKey(sr => sr.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sr => sr.Order)
                    .WithMany()
                    .HasForeignKey(sr => sr.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ExpireAt);
                entity.HasIndex(e => e.IsReleased);
                entity.HasIndex(e => new { e.OrderId, e.IsReleased });
                entity.HasIndex(e => e.ProductVariantId);
            });

            // ============================================
            // SEED DATA - Dados iniciais
            // ============================================
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Eletrônicos",
                    Description = "Produtos eletrônicos em geral",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = 2,
                    Name = "Roupas",
                    Description = "Vestuário e acessórios",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = 3,
                    Name = "Livros",
                    Description = "Livros físicos e digitais",
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Notebook Gamer",
                    Description = "Notebook de alta performance para jogos",
                    Price = 4999.90m,
                    Stock = 10,
                    ImageUrl = "https://via.placeholder.com/300",
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Id = 2,
                    Name = "Camiseta Básica",
                    Description = "Camiseta 100% algodão",
                    Price = 49.90m,
                    Stock = 50,
                    ImageUrl = "https://via.placeholder.com/300",
                    CategoryId = 2,
                    CreatedAt = DateTime.UtcNow
                }
            );

            modelBuilder.Entity<ProductVariant>().HasData(
                new ProductVariant
                {
                    Id = 1,
                    ProductId = 2,
                    Size = "P",
                    Stock = 10,
                    ReservedStock = 0,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductVariant
                {
                    Id = 2,
                    ProductId = 2,
                    Size = "M",
                    Stock = 15,
                    ReservedStock = 0,
                    CreatedAt = DateTime.UtcNow
                },
                new ProductVariant
                {
                    Id = 3,
                    ProductId = 2,
                    Size = "G",
                    Stock = 12,
                    ReservedStock = 0,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
