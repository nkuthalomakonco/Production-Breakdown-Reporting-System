using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BreakdownManager.Data;

/// <summary>
/// Lets you run `dotnet ef migrations add ...` / `dotnet ef database update` directly
/// against this project without needing the WPF app to build first.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BreakdownManagerDbContext>
{
    public BreakdownManagerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BreakdownManagerDbContext>();
        optionsBuilder.UseSqlite("Data Source=BreakdownManager.db");
        return new BreakdownManagerDbContext(optionsBuilder.Options);
    }
}
