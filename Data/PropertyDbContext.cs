using Microsoft.EntityFrameworkCore;
using PersonalPropertyManager.Models;

namespace PersonalPropertyManager.Data;

/// <summary>
/// EF Core database context for the personal property database.
/// Backed by SQLite. The DB file is created/updated automatically on first run.
/// </summary>
public class PropertyDbContext : DbContext
{
    public DbSet<PersonalProperty> Properties => Set<PersonalProperty>();

    public PropertyDbContext(DbContextOptions<PropertyDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PersonalProperty>(entity =>
        {
            entity.ToTable("Properties");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(4000);
            entity.Property(p => p.CurrentValue).HasColumnType("decimal(18,2)");
            entity.Property(p => p.ImagePath).HasMaxLength(500);
            entity.Property(p => p.Notes).HasMaxLength(2000);
            entity.Property(p => p.Location).HasMaxLength(200);

            // Indexes help search/sort when the DB grows
            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => p.ItemType);
            entity.HasIndex(p => p.DesireStatus);
            entity.HasIndex(p => p.CurrentValue);
        });

        // Seed a few example rows so the user sees data immediately on first launch.
        modelBuilder.Entity<PersonalProperty>().HasData(
            new PersonalProperty
            {
                Id = 1,
                Name = "MacBook Pro 16\"",
                Description = "Primary work machine. Used for .NET 9 / WPF development, ML experiments, and client work. " +
                              "Skills developed on this machine: C#, F#, WPF, EF Core, SQL, debugging.",
                CurrentValue = 2499.00m,
                DesireStatus = DesireStatus.Needed,
                ItemType = ItemType.Electronics,
                AcquiredDate = new DateTime(2023, 6, 15),
                Location = "Home Office",
                Notes = "AppleCare until 2026-06-15.",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new PersonalProperty
            {
                Id = 2,
                Name = "Herman Miller Aeron (Size B)",
                Description = "Primary desk chair. Used daily for ~8 hours of focused work.",
                CurrentValue = 1450.00m,
                DesireStatus = DesireStatus.None,
                ItemType = ItemType.Furniture,
                AcquiredDate = new DateTime(2022, 1, 10),
                Location = "Home Office",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new PersonalProperty
            {
                Id = 3,
                Name = "Sony WH-1000XM5 Headphones",
                Description = "Active noise cancelling. Used for focus music and call work.",
                CurrentValue = 380.00m,
                DesireStatus = DesireStatus.None,
                ItemType = ItemType.Electronics,
                AcquiredDate = new DateTime(2024, 2, 1),
                Location = "Travel bag",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            },
            new PersonalProperty
            {
                Id = 4,
                Name = "Leatherman Wave+",
                Description = "Multi-tool carried daily. Skills: general repair, electronics disassembly.",
                CurrentValue = 109.95m,
                DesireStatus = DesireStatus.Wanted,
                ItemType = ItemType.Tool,
                AcquiredDate = new DateTime(2021, 11, 5),
                Location = "EDC",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }
        );
    }
}
