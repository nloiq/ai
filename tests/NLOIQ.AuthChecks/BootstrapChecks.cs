using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLOIQ.Infrastructure.Data;
using NLOIQ.Infrastructure.Identity;

internal static class BootstrapChecks
{
    public static async Task RunAsync(IServiceProvider services, NloDbContext db)
    {
        if (!Console.IsInputRedirected) throw new InvalidOperationException("Run with redirected stdin; no interactive credential input is used in this check.");
        var originalInput = Console.In;
        var originalOutput = Console.Out;
        var counts = (await db.Users.CountAsync(), await db.Roles.CountAsync(), await db.UserRoles.CountAsync());
        await using (var transaction = await db.Database.BeginTransactionAsync())
        {
            try
            {
                // All changes, including temporary removal of role links, remain inside this rollback.
                await db.UserRoles.ExecuteDeleteAsync();
                await db.RoleClaims.ExecuteDeleteAsync();
                await db.Roles.ExecuteDeleteAsync();
                var email = $"bootstrap-{Guid.NewGuid():N}@example.invalid";
                var password = "Aa7!" + Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(20));
                Console.SetOut(TextWriter.Null);
                Console.SetIn(new StringReader(email + "\n" + password + "\n"));
                await NLOIQ.Web.Security.AdminBootstrap.RunAsync(services);
                var manager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var user = await manager.FindByEmailAsync(email) ?? throw new Exception("Bootstrap created no user.");
                if (!await manager.CheckPasswordAsync(user, password) || !await manager.IsInRoleAsync(user, "SuperAdmin"))
                    throw new Exception("Bootstrap credentials or role failed.");
                var roles = await db.Roles.Select(x => x.Name).ToListAsync();
                if (roles.Count != 3 || !new[] { "SuperAdmin", "Editor", "Viewer" }.All(roles.Contains))
                    throw new Exception("Fresh roles were not initialized.");
                Console.SetIn(new StringReader(email + "\n"));
                var refused = false;
                try { await NLOIQ.Web.Security.AdminBootstrap.RunAsync(services); }
                catch (InvalidOperationException ex) when (ex.Message.StartsWith("Account already exists")) { refused = true; }
                if (!refused || !await manager.CheckPasswordAsync(user, password)) throw new Exception("Existing account was changed.");
                if (await db.ContentItems.CountAsync() == 0) throw new Exception("Expected existing dedicated development fixtures.");
            }
            finally
            {
                Console.SetIn(originalInput);
                Console.SetOut(originalOutput);
                await transaction.RollbackAsync();
                db.ChangeTracker.Clear();
            }
        }
        if (counts != (await db.Users.CountAsync(), await db.Roles.CountAsync(), await db.UserRoles.CountAsync()))
            throw new Exception("Bootstrap rollback did not preserve account inventory.");
        Console.WriteLine("PASS: missing-role bootstrap, password verification, SuperAdmin assignment, existing-account refusal; account inventory restored by rollback.");
    }
}
