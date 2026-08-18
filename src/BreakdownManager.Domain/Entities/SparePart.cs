namespace BreakdownManager.Domain.Entities;

public class SparePart
{
    public int Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public string? Supplier { get; set; }
    public decimal? Cost { get; set; }
    public string? Location { get; set; }

    public bool IsBelowMinimum => Stock < MinimumStock;

    public ICollection<BreakdownSparePart> BreakdownUsages { get; set; } = new List<BreakdownSparePart>();
}
