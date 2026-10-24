using Microsoft.EntityFrameworkCore;
using NLOIQ.Domain.Content;
namespace NLOIQ.Infrastructure.Data;
internal static class ScheduleCancellation
{
    internal static async Task PendingAsync(NloDbContext db,long id,Guid actor,string label,string reason,long? except,CancellationToken ct)
    {
        var rows=await db.Set<PublicationSchedule>().Where(x=>x.ContentItemId==id&&x.State==ScheduleState.Pending&&x.Id!=except).ToListAsync(ct);
        foreach(var row in rows){row.State=ScheduleState.Cancelled;row.CompletedUtc=DateTime.UtcNow;row.FailureCode=reason;}
        if(rows.Count>0)db.AuditLogs.Add(new(){ContentItemId=id,ActorUserId=actor,ActorLabel=label[..Math.Min(label.Length,160)],Action="CancelSchedules",DetailsJson=System.Text.Json.JsonSerializer.Serialize(new{reason,count=rows.Count})});
    }
}
