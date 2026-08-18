using System.Windows;
using BreakdownManager.App.ViewModels;
using BreakdownManager.Data;
using BreakdownManager.Services;
using BreakdownManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BreakdownManager.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Ensure the SQLite file exists and has demo data on first run.
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BreakdownManagerDbContext>();
            DbSeeder.Seed(db);
        }

        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddDbContext<BreakdownManagerDbContext>(options =>
            options.UseSqlite("Data Source=BreakdownManager.db"));

        services.AddScoped<IBreakdownService, BreakdownService>();
        services.AddScoped<IMachineService, MachineService>();
        services.AddScoped<IUserService, UserService>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<ReportBreakdownViewModel>();
        services.AddTransient<TechnicianDashboardViewModel>();

        services.AddTransient<MainWindow>();
    }
}
