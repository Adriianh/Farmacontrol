using Farmacontrol.Model;
using Farmacontrol.Model.ProductEntity;
using Farmacontrol.Model.UserEntity;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Repository
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<SaleDetail> SaleDetails { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<Batch> Batches { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseDetail> PurchaseDetails { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public AppDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "farmacontrol.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.Ignore("ExpirationDate");

                entity.HasDiscriminator<string>("Discriminator")
                    .HasValue<Cosmetic>("Cosmetico")
                    .HasValue<Medicine>("Medicamento")
                    .HasValue<Supplement>("Suplemento")
                    .HasValue<Supply>("Suministro");

                entity.HasIndex(e => e.Name);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Username);

                entity.HasDiscriminator<string>("Discriminator")
                    .HasValue<Administrator>("Administrador")
                    .HasValue<Employee>("Empleado");

                entity.Ignore(u => u.Role);
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.Metadata.FindNavigation(nameof(Sale.Details))?.SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.HasMany(e => e.Details).WithOne().HasForeignKey("SaleCode").OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.Property<int>("Id").ValueGeneratedOnAdd();
                entity.HasKey("Id");
            });

            modelBuilder.Entity<Batch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(b => b.Product).WithMany(p => p.Batches).HasForeignKey(b => b.ProductCode)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Purchase>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Metadata.FindNavigation(nameof(Purchase.Details))
                    ?.SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.HasMany(e => e.Details).WithOne(d => d.Purchase).HasForeignKey(d => d.PurchaseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Supplier).WithMany().HasForeignKey(e => e.SupplierCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PurchaseDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductCode)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Sale).WithOne(s => s.Prescription).HasForeignKey<Prescription>(e => e.SaleCode)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Code);
                entity.HasIndex(e => e.Name);
            });

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Suppliers)
                .WithMany(s => s.Products);

            modelBuilder.Entity<Alert>(entity =>
            {
                entity.Property<int>("Id").ValueGeneratedOnAdd();
                entity.HasKey("Id");
            });
        }
    }
}