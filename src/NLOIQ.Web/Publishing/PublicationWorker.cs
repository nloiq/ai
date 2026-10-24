using NLOIQ.Application.Content;
namespace NLOIQ.Web.Publishing;
public sealed class PublicationWorker(IServiceScopeFactory scopes,ILogger<PublicationWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromSeconds(15));
        do {
            try {
                IReadOnlyList<long> ids;
                await using(var scope=scopes.CreateAsyncScope())ids=await scope.ServiceProvider.GetRequiredService<IContentWorkflow>().DueAsync(stoppingToken);
                foreach(var id in ids){await using var scope=scopes.CreateAsyncScope();await scope.ServiceProvider.GetRequiredService<IContentWorkflow>().ExecuteAsync(id,stoppingToken);}
            }catch(OperationCanceledException)when(stoppingToken.IsCancellationRequested){break;}
            catch(Exception error){logger.LogWarning("Scheduled publication sweep failed: {ErrorType}",error.GetType().Name);}
        }while(await timer.WaitForNextTickAsync(stoppingToken));
    }
}
