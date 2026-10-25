namespace NLOIQ.Domain.Entities;
public sealed class ContactInquiry
{
    public long Id { get; set; }
    public Guid SubmissionId { get; set; }
    public DateTime CreatedUtc { get; set; }
    public string Culture { get; set; } = "en";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Company { get; set; } = "";
    public string Service { get; set; } = "general";
    public string Message { get; set; } = "";
    public bool IsRead { get; set; }
}
