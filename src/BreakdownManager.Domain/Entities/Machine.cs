using BreakdownManager.Domain.Enums;

namespace BreakdownManager.Domain.Entities;

public class Machine
{
    public int Id { get; set; }

    /// <summary>Human/plant code, e.g. "BAT-101".</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string? Area { get; set; }
    public string? Line { get; set; }
    public string? Manufacturer { get; set; }
    public string? Plc { get; set; }
    public string? Robot { get; set; }
    public DateTime? InstallationDate { get; set; }
    public Criticality Criticality { get; set; } = Criticality.Medium;
    public bool IsActive { get; set; } = true;

    public ICollection<Breakdown> Breakdowns { get; set; } = new List<Breakdown>();
}
