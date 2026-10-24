using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
public sealed class QueryCounter:DbCommandInterceptor
{
    public int Count {get;private set;}
    public void Reset()=>Count=0;
    public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,CommandEventData eventData,InterceptionResult<DbDataReader> result){Count++;return result;}
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,CommandEventData eventData,InterceptionResult<DbDataReader> result,CancellationToken cancellationToken=default){Count++;return ValueTask.FromResult(result);}
}
