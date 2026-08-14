using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonalPropertyManager.Data;
using PersonalPropertyManager.Services;
using PersonalPropertyManager.ViewModels;
using PersonalPropertyManager.Views;

namespace PersonalPropertyManager;

/// <summary>
/// Application entry point. Wires up DI, the EF Core DbContext, and shows MainWindow.
/// </summary>
public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; }

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // EF Core 9 DbContext — SQLite, file lives next to the .exe.
        // Override via PPM_DB env var if you want a different location.
        var dbPath = Environment.GetEnvironmentVariable("PPM_DB")
            ?? Path.Combine(AppContext.BaseDirectory, "app.db");

        services.AddDbContext<PropertyDbContext>(options =>
        {
            options.UseSqlite($"Data Source={dbPath}");
        });

        // Domain service
        services.AddTransient<IPropertyService, PropertyService>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Views
        services.AddTransient<MainWindow>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Make sure the database exists and is seeded before the UI binds.
        using (var scope = ServiceProvider.CreateScope())
        {
            var ctx = scope.ServiceProvider.GetRequiredService<PropertyDbContext>();
            DbInitializer.Initialize(ctx);
        }

        var window = ServiceProvider.GetRequiredService<MainWindow>();
        window.Show();
    }
}
