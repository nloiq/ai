using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using NLOIQ.Infrastructure;
using NLOIQ.Infrastructure.Data;
using NLOIQ.Infrastructure.Identity;
using var local = JsonDocument.Parse(File.ReadAllText("src/NLOIQ.Web/appsettings.Local.json"));
var connection = local.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString()!;
if (!new MySqlConnectionStringBuilder(connection).Database.Equals("WebSiteNLOIQDb", StringComparison.OrdinalIgnoreCase)) throw new Exception("Only dedicated development database permitted.");
var services = new ServiceCollection();
services.AddLogging(); services.AddInfrastructure(connection);
services.AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<NloDbContext>();
await using var provider = services.BuildServiceProvider();
await using var scope = provider.CreateAsyncScope();
var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var db = scope.ServiceProvider.GetRequiredService<NloDbContext>();
var created = new List<ApplicationUser>();
var accounts = new Dictionary<string, object>();
try {
    foreach (var name in new[] { "SuperAdmin", "Editor", "Viewer", "Disabled", "Lockout", "Revoked" }) {
        var email = $"auth-check-{Guid.NewGuid():N}@example.invalid";
        var password = "Aa7!" + Convert.ToHexString(RandomNumberGenerator.GetBytes(18));
        var user = new ApplicationUser { Id=Guid.NewGuid(), UserName=email, Email=email, IsActive=name!="Disabled" };
        var result = await users.CreateAsync(user,password);
        if(!result.Succeeded) throw new Exception("Fixture account could not be created.");
        created.Add(user);
        await users.AddToRoleAsync(user,name is "Disabled" or "Lockout" or "Revoked" ? "Viewer" : name);
        accounts[name] = new { email,password };
    }
    async Task Run(string mode) {
        var start = new ProcessStartInfo("node", "tests/auth-browser.cjs") { UseShellExecute=false };
        start.Environment["NLO_AUTH_FIXTURES"] = JsonSerializer.Serialize(accounts);
        start.Environment["NLO_AUTH_MODE"] = mode;
        using var process = Process.Start(start)!; await process.WaitForExitAsync();
        if(process.ExitCode!=0) throw new Exception("Authentication browser checks failed.");
    }
    await Run("main");
    var locked = await users.FindByIdAsync(created.Single(x => x.Email == ((JsonElement)JsonSerializer.SerializeToElement(accounts["Lockout"])).GetProperty("email").GetString()).Id.ToString());
    if (locked is not null) await db.Entry(locked).ReloadAsync();
    if (locked is null || !await users.IsLockedOutAsync(locked)) throw new Exception("Five failed passwords did not lock account.");
    Console.WriteLine("PASS: real Identity account lockout persisted");
    var revoked = created.Single(x => x.Email == JsonSerializer.SerializeToElement(accounts["Revoked"]).GetProperty("email").GetString());
    revoked.IsActive=false; await users.UpdateAsync(revoked);
    await Run("revoked");
} finally {
    foreach(var user in created) {
        db.Entry(user).State = EntityState.Detached;
        var fresh=await users.FindByIdAsync(user.Id.ToString());
        if(fresh is not null) { var deleted=await users.DeleteAsync(fresh); if(!deleted.Succeeded) throw new Exception("Fixture cleanup failed."); }
    }
    Console.WriteLine("Temporary authentication accounts removed.");
}
