using ContactApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ContactApi.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contact>(entity =>
        {
            // Configure the Contact entity
            // Set the primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(32);
            entity.HasIndex(e => e.PhoneNumber).IsUnique();
        });
    }
}
