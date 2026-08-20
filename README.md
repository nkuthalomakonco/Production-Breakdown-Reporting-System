# Breakdown Manager

A free, open-source **Production Breakdown Reporting System** for factories: production
supervisors report a machine breakdown in under a minute, technicians pick it up and work it
through a real repair workflow, and maintenance managers get MTTR/MTBF/downtime KPIs — without
the weight of a full CMMS like SAP PM or Maximo.

This repo is the **v1 WPF desktop skeleton**: a working, compilable MVP scaffold with the core
domain model, database, business logic, and the two primary screens (Supervisor report screen,
Technician dashboard) already wired together end-to-end.

> **Note on where this was built:** this scaffold was generated in a Linux sandbox, so the code
> has not been compiled or run yet — WPF only builds/runs on Windows. Everything is written to
> compile cleanly in Visual Studio 2022 on Windows, but budget time for the first build to shake
> out any small issues (a missing `using`, a NuGet version bump, etc.) before you rely on it.
>
> A code review since the initial scaffold caught and fixed one issue that would have crashed the
> app on first launch: the seeded demo users had duplicate `Username` values, which violated the
> unique index on `Users.Username` and threw during `DbSeeder.Seed()`. That's fixed. The repo also
> now has a proper `.gitignore` so `bin/`, `obj/`, and the `.vs/` folder stay out of source
> control going forward.

## Getting started (Windows)

1. Install **Visual Studio 2022 Community** (or later) with the **.NET desktop development**
   workload, which includes .NET 9 and WPF tooling.
2. Clone the repo and open `BreakdownManager.sln`.
3. Visual Studio will restore the NuGet packages listed in each `.csproj` on first load
   (EF Core Sqlite, CommunityToolkit.Mvvm, xUnit, etc.) — no manual install needed if you have
   an internet connection.
4. Set `BreakdownManager.App` as the startup project (it already is, by convention — the only
   executable project) and press **F5**.
5. On first run the app creates `BreakdownManager.db` (a SQLite file) next to the executable and
   seeds it with a few demo machines and users (2 supervisors, 2 technicians, 1 maintenance
   manager) so the screens aren't empty.

No separate database server, no Docker, nothing else to install — that's the point of SQLite here.

### If you'd rather use EF Core migrations instead of `EnsureCreated`

The scaffold currently seeds via `Database.EnsureCreated()` for zero-friction first runs. Once
you start evolving the schema, switch to proper migrations:

```
cd src/BreakdownManager.Data
dotnet ef migrations add InitialCreate --startup-project ../BreakdownManager.App
dotnet ef database update --startup-project ../BreakdownManager.App
```

(`DesignTimeDbContextFactory.cs` is already in place so `dotnet ef` works without the app running.)

## Project structure

```
BreakdownManager/
├── BreakdownManager.sln
└── src/
    ├── BreakdownManager.Domain/     Entities & enums — no dependencies on anything else
    ├── BreakdownManager.Data/       EF Core DbContext, SQLite config, seeding
    ├── BreakdownManager.Services/   Business logic (ticket workflow, KPI calculation)
    ├── BreakdownManager.App/        WPF UI (MVVM, CommunityToolkit.Mvvm)
    └── BreakdownManager.Tests/      xUnit tests against a real in-memory SQLite connection
```

Dependency direction is strictly one-way: `App → Services → Data → Domain`. Nothing in `Domain`
or `Data` knows the app is a desktop app — that matters later if you build the ASP.NET Core API
/ Blazor PWA version, since `Services` and `Domain` can be reused as-is.

## What's implemented in this v1

- **Domain model**: `Machine`, `User` (Supervisor/Technician/Manager roles), `Breakdown` (full
  ticket lifecycle with timestamps per stage), `SparePart`, `Attachment`.
- **Ticket workflow**: New → Assigned → Travelling → Diagnosing → Waiting for Parts → Repairing
  → Testing → Completed → Closed, exactly as scoped — not a generic Open/Closed helpdesk model.
- **Supervisor screen**: pick machine, category, priority, description, optional photo, submit —
  designed to take under a minute.
- **Technician dashboard**: see unassigned jobs, accept one, and advance it through the workflow
  stages with a single button per stage.
- **Auto-computed KPIs** on every ticket: response time (report → assigned), MTTR (report →
  completed), and downtime — plus a `DashboardStats` aggregate (open/in-progress/waiting-parts
  counts, average MTTR, top problem machines) ready to bind to a dashboard view.
- **Ticket numbering**: `BD-2026-0001` style, auto-incrementing per year.
- **Tests**: ticket creation, technician assignment, and repair completion, run against a real
  (in-memory) SQLite connection so the actual SQL translation is exercised, not just an in-memory
  fake.

## What's deliberately not in v1 yet

These are straightforward to layer on top of the current structure, in roughly this order:

1. **Login / auth** — right now "who's using the app" is a dropdown picker standing in for a
   real login, exactly so a proper login screen can slot in later without touching the
   ViewModels.
2. **Manager dashboard & charts** (LiveCharts2) — `IBreakdownService.GetDashboardStatsAsync()`
   already returns everything a dashboard needs; it just doesn't have a screen yet.
3. **PDF/Excel reports** (QuestPDF / ClosedXML).
4. **Notifications** (Windows toast when a job is accepted / when a machine's been down >30 min).
5. **Root cause tooling** (5 Whys / Fishbone) beyond the current free-text `RootCause` field.
6. **Spare parts consumption UI** — the `BreakdownSparePart` join entity and stock tracking on
   `SparePart` already exist in the data model; there's no screen for it yet.
7. Longer term: QR codes per machine, PLC alarm integration, and the ASP.NET Core API + Blazor
   PWA path discussed for v2/v3 so supervisors and technicians can use it from tablets/phones.

## License

MIT — see `LICENSE`.
