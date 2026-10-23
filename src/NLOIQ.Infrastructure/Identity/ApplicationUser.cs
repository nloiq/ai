using Microsoft.AspNetCore.Identity;
namespace NLOIQ.Infrastructure.Identity;
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
