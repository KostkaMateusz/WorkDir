using Microsoft.EntityFrameworkCore;
using WorkDir.Domain.Entities;

namespace WorkDir.API.Entities;

public class WorkContext : DbContext
{
    public WorkContext(DbContextOptions<WorkContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Share> Shares { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(eb =>
        {
            eb.Property(user => user.CreationDate).HasDefaultValueSql("getutcdate()");
            eb.HasMany(u => u.Items).WithOne(i => i.User).OnDelete(DeleteBehavior.ClientCascade);
            eb.HasMany(u => u.Shares).WithOne(s => s.SharedWith);
        });

        modelBuilder.Entity<Item>(eb =>
        {
            eb.HasOne(i => i.User).WithMany(u => u.Items).HasForeignKey(i => i.OwnerId);
            eb.HasMany(i => i.Shares).WithOne(s => s.SharedItem);

        });

        modelBuilder.Entity<Share>(eb =>
        {
            eb.HasOne(s => s.SharedWith).WithMany(u => u.Shares).HasForeignKey(s => s.SharedWithId);
            eb.HasOne(s => s.SharedItem).WithMany(i => i.Shares).HasForeignKey(s => s.SharedItemId);
            eb.HasKey(s => new { s.SharedWithId, s.SharedItemId });
        });
    }
}
