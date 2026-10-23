using Microsoft.EntityFrameworkCore;
using NLOIQ.Domain.Entities;
namespace NLOIQ.Infrastructure.Data;
public sealed class DevelopmentSeeder(NloDbContext database)
{
    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        var content = new[] {
            new[] { "low-current", "Low-current engineering", "Connectivity and low-current infrastructure, coordinated as part of a complete system.", "هندسة التيار الخفيف", "الربط والبنية التحتية للتيار الخفيف ضمن منظومة متكاملة." },
            new[] { "software", "Software solutions", "Software designed around the information, interfaces, and workflows your system requires.", "الحلول البرمجية", "برمجيات تُصمّم وفق المعلومات والواجهات وسير العمل الذي تتطلّبه منظومتك." },
            new[] { "scada", "SCADA system integration", "Bring supervisory monitoring, process information, and control interfaces into a coordinated environment.", "تكامل أنظمة SCADA", "ربط المراقبة الإشرافية ومعلومات العمليات وواجهات التحكّم في بيئة متناسقة." },
            new[] { "automation", "System automation", "Connect system inputs, control logic, and outputs in a clearly structured automation workflow.", "أتمتة الأنظمة", "ربط مدخلات النظام ومنطق التحكّم والمخرجات ضمن سير عمل واضح للأتمتة." }
        };
        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var inserted = 0;
        for (var i = 0; i < content.Length; i++)
        {
            var row = content[i];
            // Add missing keys only; never overwrite existing content on rerun.
            if (await database.ServicePreviews.AnyAsync(x => x.Key == row[0], cancellationToken)) continue;
            database.ServicePreviews.Add(new ServicePreview {
                Key = row[0], DisplayOrder = i + 1, IsDemo = true,
                Translations = [
                    new() { Culture = "en", Title = row[1], Summary = row[2] },
                    new() { Culture = "ar", Title = row[3], Summary = row[4] }
                ]
            });
            inserted++;
        }
        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return inserted;
    }
}
