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
                
                entity.HasDiscriminator<string>("Discriminator")
                    .HasValue<Cosmetic>("Cosmetico")
                    .HasValue<Medicine>("Medicamento")
                    .HasValue<Supplement>("Suplemento")
                    .HasValue<Supply>("Suministro");
                    
                
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
                // Allow EF to navigate the private Details list
                entity.Metadata.FindNavigation(nameof(Sale.Details))?.SetPropertyAccessMode(PropertyAccessMode.Field);
                entity.HasMany(e => e.Details).WithOne().HasForeignKey("SaleCode").OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SaleDetail>(entity =>
            {
                entity.Property<int>("Id").ValueGeneratedOnAdd();
                entity.HasKey("Id");
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.Code);
            });

            modelBuilder.Entity<Alert>(entity =>
            {
                entity.Property<int>("Id").ValueGeneratedOnAdd();
                entity.HasKey("Id");
            });
        }
    }
}
