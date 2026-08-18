namespace BreakdownManager.Domain.Entities;

/// <summary>Join entity: which spare parts (and how many) were used on a breakdown.</summary>
public class BreakdownSparePart
{
    public int BreakdownId { get; set; }
    public Breakdown Breakdown { get; set; } = null!;

    public int SparePartId { get; set; }
    public SparePart SparePart { get; set; } = null!;

    public int QuantityUsed { get; set; }
}
