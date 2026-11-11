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
                new User { FullName = "Nkuthalo Makonco", Username = "admin", Role = UserRole.Supervisor, PasswordHash = PasswordHasher.Hash("admin") },
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

        SeedDemoBreakdowns(context);
    }

    /// <summary>
    /// Adds a couple of weeks of realistic-looking tickets so the manager dashboard's charts have
    /// something to show on a fresh install. Only runs when the Breakdowns table is empty, so it
    /// never touches real data.
    /// </summary>
    private static void SeedDemoBreakdowns(BreakdownManagerDbContext context)
    {
        if (context.Breakdowns.Any())
            return;

        var machines = context.Machines.OrderBy(m => m.Id).ToList();
        var supervisor = context.Users.First(u => u.Role == UserRole.Supervisor);
        var technicians = context.Users.Where(u => u.Role == UserRole.Technician).OrderBy(u => u.Id).ToList();
        if (machines.Count == 0 || technicians.Count == 0)
            return;

        // (days ago, machine index, category, priority, hours to assign, hours to complete or null if still open, status if open)
        var samples = new (int DaysAgo, int Machine, FaultCategory Category, Priority Priority, double AssignHours, double? RepairHours, BreakdownStatus OpenStatus, string Text)[]
        {
            (13, 0, FaultCategory.Mechanical, Priority.High,     0.4, 3.5,  BreakdownStatus.Closed, "Paste hopper feed roller seized"),
            (12, 2, FaultCategory.Electrical, Priority.Medium,   0.8, 2.0,  BreakdownStatus.Closed, "Conveyor motor contactor tripping"),
            (10, 1, FaultCategory.Plc,        Priority.Critical, 0.2, 6.0,  BreakdownStatus.Closed, "Formation rectifier comms fault E204"),
            (9,  0, FaultCategory.Mechanical, Priority.Medium,   1.1, 4.2,  BreakdownStatus.Closed, "Grid feed misalignment"),
            (8,  3, FaultCategory.Hydraulic,  Priority.High,     0.5, 5.0,  BreakdownStatus.Closed, "Press pressure dropping under load"),
            (7,  0, FaultCategory.Mechanical, Priority.Low,      2.0, 1.5,  BreakdownStatus.Closed, "Guard interlock sticking"),
            (6,  2, FaultCategory.Pneumatic,  Priority.Medium,   0.6, 2.4,  BreakdownStatus.Closed, "Air line leak at diverter"),
            (5,  0, FaultCategory.Robot,      Priority.High,     0.3, 7.5,  BreakdownStatus.Closed, "KUKA arm collision fault"),
            (4,  1, FaultCategory.Electrical, Priority.Medium,   0.9, 3.0,  BreakdownStatus.Closed, "Formation channel 12 not charging"),
            (3,  0, FaultCategory.Mechanical, Priority.High,     0.4, 4.8,  BreakdownStatus.Closed, "Pasting belt tracking off"),
            (2,  2, FaultCategory.Software,   Priority.Low,      1.5, 0.8,  BreakdownStatus.Closed, "HMI recipe reset after power dip"),
            (1,  0, FaultCategory.Mechanical, Priority.High,     0.5, null, BreakdownStatus.Repairing,       "Pasting roller bearing noise"),
            (1,  3, FaultCategory.Hydraulic,  Priority.Medium,   1.0, null, BreakdownStatus.WaitingForParts, "Hydraulic hose weeping"),
            (0,  1, FaultCategory.Plc,        Priority.Critical, 0.2, null, BreakdownStatus.Diagnosing,      "Line 3 PLC watchdog trip"),
            (0,  2, FaultCategory.Electrical, Priority.Low,      0.0, null, BreakdownStatus.New,             "Panel light flickering"),
        };

        var perYearCounter = new Dictionary<int, int>();
        var index = 0;

        foreach (var sample in samples)
        {
            var reportedAt = DateTime.Today.AddDays(-sample.DaysAgo).AddHours(6 + (index % 9));
            if (reportedAt > DateTime.Now)
                reportedAt = DateTime.Now.AddMinutes(-30 - index);

            perYearCounter[reportedAt.Year] = perYearCounter.GetValueOrDefault(reportedAt.Year) + 1;

            var breakdown = new Breakdown
            {
                TicketNumber = $"BD-{reportedAt.Year}-{perYearCounter[reportedAt.Year]:D4}",
                MachineId = machines[sample.Machine % machines.Count].Id,
                ReportedByUserId = supervisor.Id,
                Category = sample.Category,
                Priority = sample.Priority,
                Description = sample.Text,
                Shift = index % 2 == 0 ? "Day" : "Night",
                ReportedAt = reportedAt,
                Status = sample.RepairHours.HasValue ? BreakdownStatus.Closed : sample.OpenStatus
            };

            if (sample.OpenStatus != BreakdownStatus.New)
            {
                breakdown.AssignedTechnicianId = technicians[index % technicians.Count].Id;
                breakdown.AssignedAt = reportedAt.AddHours(sample.AssignHours);
            }

            if (sample.RepairHours.HasValue)
            {
                breakdown.CompletedAt = reportedAt.AddHours(sample.RepairHours.Value);
                breakdown.ClosedAt = breakdown.CompletedAt;
                breakdown.RootCause = "Demo data";
                breakdown.RepairActions = "Demo data";
            }

            context.Breakdowns.Add(breakdown);
            index++;
        }

        context.SaveChanges();
    }
}
