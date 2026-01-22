using Microsoft.EntityFrameworkCore;
using Olimpo.ProductAPI.Model.Entities;

namespace Olimpo.ProductAPI.Model.Context
{
    public class MySQLContext : DbContext
    {
        public MySQLContext(DbContextOptions<MySQLContext> options) : base(options) { }

        // DbSets - Representam as tabelas no banco
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

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

                // Relacionamento: Product -> Category
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict); // Não permite deletar categoria com produtos

                // Índices para performance
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.CategoryId);
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
        }
    }
}
