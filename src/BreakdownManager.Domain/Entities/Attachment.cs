namespace BreakdownManager.Domain.Entities;

public enum AttachmentType
{
    Photo,
    Video,
    PlcBackup,
    Pdf,
    Other
}

public class Attachment
{
    public int Id { get; set; }
    public int BreakdownId { get; set; }
    public Breakdown Breakdown { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty;
    public AttachmentType Type { get; set; } = AttachmentType.Photo;
    public DateTime UploadedAt { get; set; } = DateTime.Now;
}
