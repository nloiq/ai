using NLOIQ.Domain.Content;
namespace NLOIQ.Application.Content;
public sealed record RevisionRow(long Id,int Number,DateTime CreatedUtc,string Actor,string ChangeNote,string TitleEn,string TitleAr,bool IsDraft,bool IsPublished);
public sealed record ScheduleRow(long Id,long RevisionId,DateTime ScheduledUtc,ScheduleState State,long Version,string Actor,string? FailureCode,int RevisionNumber);
public sealed record WorkflowHistory(long Id,ContentKind Kind,string Key,long Version,PublicationState State,long? DraftRevisionId,long? PublishedRevisionId,IReadOnlyList<RevisionRow> Revisions,IReadOnlyList<ScheduleRow> Schedules,int Page,bool HasNext);
public interface IContentWorkflow
{
    Task<WorkflowHistory?> HistoryAsync(long id,int page=1,CancellationToken ct=default);
    Task RestoreAsync(long id,long revisionId,long version,Guid actor,string label,CancellationToken ct=default);
    Task ScheduleAsync(long id,long revisionId,long version,DateTime dueUtc,bool verified,Guid actor,string label,CancellationToken ct=default);
    Task CancelAsync(long id,long scheduleId,long version,long scheduleVersion,Guid actor,string label,CancellationToken ct=default);
    Task<IReadOnlyList<long>> DueAsync(CancellationToken ct=default);
    Task ExecuteAsync(long scheduleId,CancellationToken ct=default);
}
public interface IContentPreviewReader { Task<PublicSite?> PreviewAsync(long id,long revisionId,string culture,CancellationToken ct=default); }
