using System.ComponentModel.DataAnnotations;
namespace NLOIQ.Web.ViewModels;
public sealed class ContactInquiryModel
{
    public Guid SubmissionId { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage="ValidationRequired"),StringLength(120,MinimumLength=2,ErrorMessage="InquiryLength")] public string Name { get; set; } = "";
    [Required(ErrorMessage="ValidationRequired"),EmailAddress(ErrorMessage="InquiryEmailInvalid"),StringLength(254,ErrorMessage="ValidationLength")] public string Email { get; set; } = "";
    [StringLength(40,ErrorMessage="ValidationLength"),RegularExpression(@"^[0-9+() .\-]*$",ErrorMessage="ValidationPattern")] public string? Phone { get; set; }
    [StringLength(160,ErrorMessage="ValidationLength")] public string? Company { get; set; }
    [Required(ErrorMessage="ValidationRequired"),RegularExpression("^(general|low-current|software|scada|automation)$",ErrorMessage="ValidationPattern")] public string Service { get; set; } = "general";
    [Required(ErrorMessage="ValidationRequired"),StringLength(4000,MinimumLength=10,ErrorMessage="InquiryLength")] public string Message { get; set; } = "";
    [Range(typeof(bool),"true","true",ErrorMessage="InquiryConsentRequired")] public bool Consent { get; set; }
    public string? Website { get; set; }
}
