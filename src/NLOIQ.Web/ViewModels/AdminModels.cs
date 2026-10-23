using System.ComponentModel.DataAnnotations;
namespace NLOIQ.Web.ViewModels;
public sealed class LoginModel
{
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = "";
    [Required, StringLength(256)] public string Password { get; set; } = "";
    public string? ReturnUrl { get; set; }
}
public sealed record AdminDashboardModel(int Content, int Drafts, int Published, int Media, int Users);
