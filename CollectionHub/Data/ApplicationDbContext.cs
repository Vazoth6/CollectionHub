using CollectionHub.Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CollectionHub.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<MyUser> MyUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<UserItem> UserItems { get; set; }

        // Configuração adicional (necessária devido à ambiguidade Sales/Purchases)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Chave composta para UserItem
            modelBuilder.Entity<UserItem>()
                .HasKey(ui => new { ui.UserId, ui.ItemId });

            // Relações UserItem
            modelBuilder.Entity<UserItem>()
                .HasOne(ui => ui.User)
                .WithMany(u => u.UserItems)
                .HasForeignKey(ui => ui.UserId);

            modelBuilder.Entity<UserItem>()
                .HasOne(ui => ui.Item)
                .WithMany(i => i.UserItems)
                .HasForeignKey(ui => ui.ItemId);

            // Relações ambíguas: MyUser (Sales/Purchases) <-> Transaction (Seller/Buyer)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Seller)
                .WithMany(u => u.Sales)
                .HasForeignKey(t => t.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Buyer)
                .WithMany(u => u.Purchases)
                .HasForeignKey(t => t.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relação Transaction -> Item
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Item)
                .WithMany(i => i.Transactions)
                .HasForeignKey(t => t.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MyUser>()
                .HasIndex(u => u.UserID)
                .IsUnique(); // Impede que dois MyUsers liguem ao mesmo IdentityUser

            // ⚠️ IMPORTANTE: Configurar que MyUser.UserID não é auto-gerado
            modelBuilder.Entity<MyUser>()
                .Property(u => u.UserID)
                .HasMaxLength(450);  // Mesmo tamanho do IdentityUser.Id
        }
    }

}
