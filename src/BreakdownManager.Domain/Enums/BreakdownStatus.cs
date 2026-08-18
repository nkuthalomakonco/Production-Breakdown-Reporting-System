namespace BreakdownManager.Domain.Enums;

// Ordered to reflect the real repair workflow, not just Open/Closed.
public enum BreakdownStatus
{
    New = 0,
    Assigned = 1,
    Travelling = 2,
    Diagnosing = 3,
    WaitingForParts = 4,
    Repairing = 5,
    Testing = 6,
    Completed = 7,
    Closed = 8
}
