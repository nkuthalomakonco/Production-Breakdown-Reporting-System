using BreakdownManager.Domain.Enums;

namespace BreakdownManager.Domain.Entities;

/// <summary>
/// The central record: a single reported breakdown, from the moment a supervisor
/// reports it through to the technician closing it out.
/// </summary>
public class Breakdown
{
    public int Id { get; set; }

    /// <summary>Human-friendly ticket number, e.g. "BD-2026-0001", shown to users instead of the raw Id.</summary>
    public string TicketNumber { get; set; } = string.Empty;

    // --- What / who ---
    public int MachineId { get; set; }
    public Machine Machine { get; set; } = null!;

    public int ReportedByUserId { get; set; }
    public User ReportedBy { get; set; } = null!;

    public int? AssignedTechnicianId { get; set; }
    public User? AssignedTechnician { get; set; }

    public string? Shift { get; set; }
    public FaultCategory Category { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public string Description { get; set; } = string.Empty;

    // --- Technical detail (optional, filled by technician or from PLC integration later) ---
    public string? AlarmNumber { get; set; }
    public string? PlcFault { get; set; }
    public string? RobotFault { get; set; }
    public string? RootCause { get; set; }
    public string? RepairActions { get; set; }

    public ICollection<BreakdownSparePart> SparePartsUsed { get; set; } = new List<BreakdownSparePart>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    // --- Workflow state ---
    public BreakdownStatus Status { get; set; } = BreakdownStatus.New;

    // --- Timestamps for every stage transition, used to compute KPIs ---
    public DateTime ReportedAt { get; set; } = DateTime.Now;
    public DateTime? AssignedAt { get; set; }
    public DateTime? TravellingAt { get; set; }
    public DateTime? DiagnosingAt { get; set; }
    public DateTime? WaitingForPartsAt { get; set; }
    public DateTime? RepairingAt { get; set; }
    public DateTime? TestingAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // --- Computed KPIs (not mapped to the database, calculated on the fly) ---

    /// <summary>Time from report to a technician being assigned.</summary>
    public TimeSpan? ResponseTime => AssignedAt.HasValue ? AssignedAt.Value - ReportedAt : null;

    /// <summary>Mean Time To Repair: from report to completion (the standard MTTR definition used here).</summary>
    public TimeSpan? Mttr => CompletedAt.HasValue ? CompletedAt.Value - ReportedAt : null;

    /// <summary>How long the machine has been (or was) down, end to end.</summary>
    public TimeSpan? Downtime =>
        (ClosedAt ?? CompletedAt) is DateTime end ? end - ReportedAt : DateTime.Now - ReportedAt;

    public bool IsOpen => Status != BreakdownStatus.Closed;
}
