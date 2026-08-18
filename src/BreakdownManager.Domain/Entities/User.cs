using BreakdownManager.Domain.Enums;

namespace BreakdownManager.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Breakdown> ReportedBreakdowns { get; set; } = new List<Breakdown>();
    public ICollection<Breakdown> AssignedBreakdowns { get; set; } = new List<Breakdown>();
}
