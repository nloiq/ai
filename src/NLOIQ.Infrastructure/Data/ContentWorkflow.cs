using Microsoft.EntityFrameworkCore;
using NLOIQ.Application.Content;
using NLOIQ.Domain.Content;
namespace NLOIQ.Infrastructure.Data;
public sealed class ContentWorkflow(NloDbContext db,IContentEditor editor,TimeProvider clock):IContentWorkflow
{
    private DateTime Now=>clock.GetUtcNow().UtcDateTime;
    private static string Label(string value)=>value[..Math.Min(value.Length,160)];
    private Task<ContentItem?> Lock(long id,CancellationToken ct)=>db.ContentItems.FromSqlInterpolated($"SELECT * FROM ContentItems WHERE Id={id} FOR UPDATE").SingleOrDefaultAsync(ct);
    private static void Version(ContentItem item,long version){if(item.Version!=version)throw new ContentConflictException();}
    private void Audit(long id,long? revision,Guid? actor,string label,string action,object? details=null)=>db.AuditLogs.Add(new(){ContentItemId=id,RevisionId=revision,ActorUserId=actor,ActorLabel=Label(label),Action=action,DetailsJson=details is null?null:System.Text.Json.JsonSerializer.Serialize(details)});
    public async Task<WorkflowHistory?> HistoryAsync(long id,int page=1,CancellationToken ct=default)
    {
        var root=await db.ContentItems.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);if(root is null)return null;page=Math.Clamp(page,1,10000);
        var revisions=await db.ContentRevisions.AsNoTracking().Where(x=>x.ContentItemId==id).OrderByDescending(x=>x.RevisionNumber).Skip((page-1)*30).Take(31).ToListAsync(ct);
        var ids=revisions.Select(x=>x.Id).ToArray();var text=await db.ContentTranslations.AsNoTracking().Where(x=>ids.Contains(x.RevisionId)).ToListAsync(ct);
        var actions=await db.AuditLogs.AsNoTracking().Where(x=>x.ContentItemId==id&&x.RevisionId!=null&&ids.Contains(x.RevisionId.Value)&&(x.Action=="CreateDraft"||x.Action=="SaveDraft"||x.Action=="RestoreRevision")).OrderBy(x=>x.Id).ToListAsync(ct);
        var rows=revisions.Take(30).Select(x=>new RevisionRow(x.Id,x.RevisionNumber,x.CreatedUtc,actions.FirstOrDefault(a=>a.RevisionId==x.Id)?.ActorLabel??"",x.ChangeNote,text.SingleOrDefault(t=>t.RevisionId==x.Id&&t.Culture=="en")?.Title??"",text.SingleOrDefault(t=>t.RevisionId==x.Id&&t.Culture=="ar")?.Title??"",root.DraftRevisionId==x.Id,root.PublishedRevisionId==x.Id)).ToList();
        var schedules=await db.Set<PublicationSchedule>().AsNoTracking().Where(x=>x.ContentItemId==id).OrderByDescending(x=>x.Id).Take(20).Select(x=>new ScheduleRow(x.Id,x.RevisionId,x.ScheduledUtc,x.State,x.Version,x.RequestedByLabel,x.FailureCode,db.ContentRevisions.Where(r=>r.Id==x.RevisionId).Select(r=>r.RevisionNumber).First())).ToListAsync(ct);
        return new(id,root.Kind,root.Key,root.Version,root.State,root.DraftRevisionId,root.PublishedRevisionId,rows,schedules,page,revisions.Count>30);
    }
    public async Task RestoreAsync(long id,long revisionId,long version,Guid actor,string label,CancellationToken ct=default)
    {
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var item=await Lock(id,ct)??throw new KeyNotFoundException();Version(item,version);
        var source=await db.ContentRevisions.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==revisionId&&x.ContentItemId==id,ct)??throw new KeyNotFoundException();
        if(source.SchemaVersion!=1)throw new InvalidOperationException("Unsupported revision schema.");
        var revision=new ContentRevision{ContentItem=item,RevisionNumber=await db.ContentRevisions.Where(x=>x.ContentItemId==id).MaxAsync(x=>x.RevisionNumber,ct)+1,SupersedesRevisionId=item.DraftRevisionId,SchemaVersion=source.SchemaVersion,CreatedByUserId=actor,ChangeNote=$"Restored revision {source.RevisionNumber}"};
        db.Add(revision);await RevisionCloner.CloneAsync(db,revisionId,revision,ct);await db.SaveChangesAsync(ct);
        await ScheduleCancellation.PendingAsync(db,id,actor,label,"DraftChanged",null,ct);
        item.DraftRevisionId=revision.Id;if(item.State==PublicationState.Archived)item.State=PublicationState.Draft;
        Audit(id,revision.Id,actor,label,"RestoreRevision",new{sourceRevisionId=revisionId});
        try{await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);}catch(DbUpdateConcurrencyException){throw new ContentConflictException();}
    }
    public async Task ScheduleAsync(long id,long revisionId,long version,DateTime dueUtc,bool verified,Guid actor,string label,CancellationToken ct=default)
    {
        if(!verified)throw new InvalidOperationException("Confirm that both translations, links and media have been reviewed before publication.");
        if(dueUtc.Kind!=DateTimeKind.Utc||dueUtc<Now.AddMinutes(1)||dueUtc>Now.AddDays(366))throw new InvalidOperationException("Schedule must be between one minute and one year ahead.");
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var item=await Lock(id,ct)??throw new KeyNotFoundException();Version(item,version);
        if(item.State==PublicationState.Archived||item.DraftRevisionId!=revisionId)throw new ContentConflictException();
        if(!await Authorized(actor,ct))throw new InvalidOperationException("Scheduled publisher is no longer authorized.");
        await ContentWriteGuard.ValidateCandidateAsync(db,new ContentItem{Id=id,Kind=item.Kind,PublishedRevisionId=revisionId,IsDemo=false},ct);
        await ScheduleCancellation.PendingAsync(db,id,actor,label,"Replaced",null,ct);
        db.Add(new PublicationSchedule{ContentItemId=id,RevisionId=revisionId,ScheduledUtc=dueUtc,State=ScheduleState.Pending,RequestedByUserId=actor,RequestedByLabel=Label(label),CreatedUtc=Now});
        db.Entry(item).Property(x=>x.Version).IsModified=true;
        Audit(id,revisionId,actor,label,"SchedulePublish",new{scheduledUtc=dueUtc});
        try{await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);}catch(DbUpdateConcurrencyException){throw new ContentConflictException();}
    }
    public async Task CancelAsync(long id,long scheduleId,long version,long scheduleVersion,Guid actor,string label,CancellationToken ct=default)
    {
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var root=await Lock(id,ct)??throw new KeyNotFoundException();Version(root,version);
        var job=await db.Set<PublicationSchedule>().SingleOrDefaultAsync(x=>x.Id==scheduleId&&x.ContentItemId==id,ct)??throw new KeyNotFoundException();
        if(job.Version!=scheduleVersion||job.State!=ScheduleState.Pending)throw new ContentConflictException();
        job.State=ScheduleState.Cancelled;job.CompletedUtc=Now;job.FailureCode="Manual";db.Entry(root).Property(x=>x.Version).IsModified=true;
        Audit(id,job.RevisionId,actor,label,"CancelSchedule",new{scheduleId});
        try{await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);}catch(DbUpdateConcurrencyException){throw new ContentConflictException();}
    }
    private Task<bool> Authorized(Guid actor,CancellationToken ct)=>db.Users.AnyAsync(user=>user.Id==actor&&user.IsActive&&(user.LockoutEnd==null||user.LockoutEnd<clock.GetUtcNow())&&db.UserRoles.Any(link=>link.UserId==user.Id&&db.Roles.Any(role=>role.Id==link.RoleId&&role.Name=="SuperAdmin")),ct);
    public async Task<IReadOnlyList<long>> DueAsync(CancellationToken ct=default)=>await db.Set<PublicationSchedule>().AsNoTracking().Where(x=>x.State==ScheduleState.Pending&&x.ScheduledUtc<=Now).OrderBy(x=>x.ScheduledUtc).ThenBy(x=>x.Id).Take(20).Select(x=>x.Id).ToListAsync(ct);
    public async Task ExecuteAsync(long scheduleId,CancellationToken ct=default)
    {
        var identity=await db.Set<PublicationSchedule>().AsNoTracking().Where(x=>x.Id==scheduleId).Select(x=>(long?)x.ContentItemId).SingleOrDefaultAsync(ct);if(identity is not long id)return;
        // Lock content first everywhere, then read the schedule: concurrent saves/publications serialize here.
        await using var tx=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var root=await Lock(id,ct);if(root is null)return;
        var job=await db.Set<PublicationSchedule>().SingleOrDefaultAsync(x=>x.Id==scheduleId,ct);
        if(job is null||job.State!=ScheduleState.Pending||job.ScheduledUtc>Now)return;
        var code=root.DraftRevisionId!=job.RevisionId||root.State==PublicationState.Archived?"DraftChanged":job.RequestedByUserId is not Guid actor||!await Authorized(actor,ct)?"PublisherUnavailable":null;
        if(code is not null){job.State=ScheduleState.Failed;job.FailureCode=code;job.CompletedUtc=Now;Audit(id,job.RevisionId,null,"Publication worker","ScheduleFailed",new{scheduleId,code});await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);return;}
        try {
            await editor.TransitionAsync(id,root.Version,"publish",true,job.RequestedByUserId!.Value,job.RequestedByLabel,ct,job.Id);
            job.State=ScheduleState.Completed;job.CompletedUtc=Now;Audit(id,job.RevisionId,job.RequestedByUserId,job.RequestedByLabel,"ScheduledPublish",new{scheduleId});
            await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);
        }catch(Exception error)when(error is InvalidOperationException or DbUpdateException) {
            if(tx is null)throw;
            await tx.RollbackAsync(ct);db.ChangeTracker.Clear();
            // A failure is recorded separately; rollback leaves the previous public revision/routes intact.
            await using var failure=await db.Database.BeginTransactionAsync(ct);await Lock(id,ct);
            var pending=await db.Set<PublicationSchedule>().SingleAsync(x=>x.Id==scheduleId,ct);
            if(pending.State==ScheduleState.Pending){pending.State=ScheduleState.Failed;pending.FailureCode="ValidationFailed";pending.CompletedUtc=Now;Audit(id,pending.RevisionId,null,"Publication worker","ScheduleFailed",new{scheduleId,code="ValidationFailed"});await db.SaveChangesAsync(ct);}
            await failure.CommitAsync(ct);
        }
    }
}
