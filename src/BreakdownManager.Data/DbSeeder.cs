using BreakdownManager.Domain.Entities;
using BreakdownManager.Domain.Enums;
using BreakdownManager.Domain.Security;

namespace BreakdownManager.Data;

/// <summary>Seeds a handful of demo machines and users so the app isn't empty on first run.</summary>
public static class DbSeeder
{
    public static void Seed(BreakdownManagerDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            // The plaintext value below each demo user is their login password — it's only
            // readable here because this is seed data; PasswordHasher.Hash() is what actually
            // gets stored in the database, exactly as a real signup would.
            context.Users.AddRange(
                new User { FullName = "Jabu Nkosi", Username = "jabu.n", Role = UserRole.Supervisor, PasswordHash = PasswordHasher.Hash("jabu") },
                new User { FullName = "John Mokoena", Username = "john.m", Role = UserRole.Technician, PasswordHash = PasswordHasher.Hash("john") },
				new User { FullName = "Nkuthalo Makonco", Username = "nkuthalo.tech", Role = UserRole.Technician, PasswordHash = PasswordHasher.Hash("nkuthalo") },
				new User { FullName = "Nkuthalo Makonco", Username = "nkuthalo.sup", Role = UserRole.Supervisor, PasswordHash = PasswordHasher.Hash("nkuthalo") },
				new User { FullName = "Nkuthalo Makonco", Username = "nkuthalo.mgr", Role = UserRole.MaintenanceManager, PasswordHash = PasswordHasher.Hash("nkuthalo") }
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
