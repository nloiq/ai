using Microsoft.AspNetCore.Identity;
using NLOIQ.Infrastructure.Identity;
namespace NLOIQ.Web.Security;
public static class AdminBootstrap
{
    public static async Task RunAsync(IServiceProvider services)
    {
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        Console.Write("Administrator email: ");
        var email = Console.ReadLine()?.Trim() ?? "";
        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            throw new InvalidOperationException("A valid email is required.");
        if (await users.FindByEmailAsync(email) is not null)
            throw new InvalidOperationException("Account already exists; no password or role was changed.");
        Console.Write("Password (14+ characters, upper/lower/digit/symbol; hidden): ");
        string password;
        if (Console.IsInputRedirected) password = Console.ReadLine() ?? "";
        else {
            var buffer = new System.Text.StringBuilder();
            ConsoleKeyInfo key;
            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter) {
                if (key.Key == ConsoleKey.Backspace) { if (buffer.Length > 0) buffer.Length--; }
                else if (!char.IsControl(key.KeyChar)) buffer.Append(key.KeyChar);
            }
            password = buffer.ToString(); Console.WriteLine();
        }
        if (password.Length > 256) throw new InvalidOperationException("Password must be at most 256 characters.");
        if (!await roles.RoleExistsAsync("SuperAdmin"))
            throw new InvalidOperationException("Initialize the database roles first.");
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = email, Email = email, DisplayName = email };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        var assigned = await users.AddToRoleAsync(user, "SuperAdmin");
        if (!assigned.Succeeded) { await users.DeleteAsync(user); throw new InvalidOperationException("Role assignment failed; account removed."); }
        Console.WriteLine("Administrator created. No password was saved to configuration or logs.");
    }
}
