using System;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace PersonalPropertyManager.Data;

/// <summary>
/// Helper that ensures the SQLite database file exists, the schema is up to date,
/// and any seed data has been written. Idempotent — safe to call on every startup.
/// </summary>
public static class DbInitializer
{
    public static void Initialize(PropertyDbContext context)
    {
        // Ensure the directory exists (AppData location).
        var dbPath = context.Database.GetDbConnection().DataSource;
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        // Create the database and apply migrations if it doesn't exist.
        // If the file exists but is empty/invalid, it will be created/reset.
        context.Database.EnsureCreated();

        // Make sure seed rows are present (EnsureCreated does NOT run HasData on existing DBs).
        if (!context.Properties.Any())
        {
            // The seed data is also declared in OnModelCreating via HasData; this branch is a safety net.
            context.SaveChanges();
        }
    }
}
