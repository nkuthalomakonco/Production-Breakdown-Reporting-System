using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;

namespace BreakdownManager.Data;

/// <summary>Seeds a handful of demo machines and users so the app isn't empty on first run.</summary>
public static class DbSeeder
{
    public static void Seed(BreakdownManagerDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User { FullName = "Jabu Nkosi", Username = "jabu.n", Role = UserRole.Supervisor, PasswordHash = "jabu" },
                new User { FullName = "John Mokoena", Username = "john.m", Role = UserRole.Technician, PasswordHash = "john" },
				new User { FullName = "Nkuthalo Makonco", Username = "nkuthalo.m", Role = UserRole.Technician, PasswordHash = "10" },
                new User { FullName = "Nkuthalo Makonco", Username = "nkuthalo.m", Role = UserRole.Supervisor, PasswordHash = "100" },
                new User { FullName = "Nkuthalo Makonco", Username = "nkuthalo.m", Role = UserRole.MaintenanceManager, PasswordHash = "1000" }
            );
        }

        if (!context.Machines.Any())
        {
            context.Machines.AddRange(
                new Machine { Code = "BAT-101", Name = "Lead Pasting Machine", Area = "Pasting", Line = "Line 1", Manufacturer = "Sovema", Plc = "Siemens S7-1500", Robot = "KUKA KR6", Criticality = Criticality.High },
                new Machine { Code = "BAT-102", Name = "Formation Line 3", Area = "Formation", Line = "Line 3", Manufacturer = "Digatron", Plc = "Allen-Bradley CompactLogix", Criticality = Criticality.High },
                new Machine { Code = "BAT-201", Name = "Conveyor - Line 3", Area = "Assembly", Line = "Line 3", Manufacturer = "In-house", Plc = "Siemens S7-1200", Criticality = Criticality.Medium },
                new Machine { Code = "BAT-305", Name = "Hydraulic Press", Area = "Casting", Line = "Line 2", Manufacturer = "Wirtz", Plc = "Schneider M340", Criticality = Criticality.Medium }
            );
        }

        context.SaveChanges();
    }
}
