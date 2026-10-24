using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NLOIQ.Domain.Content;
namespace NLOIQ.Infrastructure.Data;
internal static class RevisionCloner
{
    private static async Task<List<object>> Rows<T>(NloDbContext db,long revision,CancellationToken ct) where T:class,IRevisionOwned =>
        (await db.Set<T>().AsNoTracking().Where(x=>x.RevisionId==revision).ToListAsync(ct)).Cast<object>().ToList();
    internal static async Task CloneAsync(NloDbContext db,long source,ContentRevision revision,CancellationToken ct)
    {
        var copies=new List<(IEntityType Type,object Original,object Copy)>();
        foreach(var type in db.Model.GetEntityTypes().Where(x=>typeof(IRevisionOwned).IsAssignableFrom(x.ClrType))) {
            var loader=typeof(RevisionCloner).GetMethod(nameof(Rows),BindingFlags.NonPublic|BindingFlags.Static)!.MakeGenericMethod(type.ClrType);
            var rows=await (Task<List<object>>)loader.Invoke(null,[db,source,ct])!;
            foreach(var row in rows) {
                var copy=(IRevisionOwned)Activator.CreateInstance(type.ClrType)!;
                foreach(var property in type.GetProperties()) {
                    if(property.Name==nameof(IRevisionOwned.RevisionId)||property.ValueGenerated==ValueGenerated.OnAdd)continue;
                    var info=property.PropertyInfo??throw new InvalidOperationException("Unsupported snapshot property.");
                    info.SetValue(copy,info.GetValue(row));
                }
                copy.Revision=revision;copies.Add((type,row,copy));
            }
        }
        foreach(var entry in copies)foreach(var key in entry.Type.GetForeignKeys().Where(x=>typeof(IRevisionOwned).IsAssignableFrom(x.PrincipalEntityType.ClrType))) {
            var values=key.Properties.Select(x=>x.PropertyInfo!.GetValue(entry.Original)).ToArray();
            if(values.Any(x=>x is null))continue;
            var target=copies.SingleOrDefault(candidate=>candidate.Type==key.PrincipalEntityType&&key.PrincipalKey.Properties.Select(x=>x.PropertyInfo!.GetValue(candidate.Original)).SequenceEqual(values));
            if(target.Copy is null)continue; // references to another revision remain pinned to that revision
            if(key.DependentToPrincipal?.PropertyInfo is { } navigation) {
                foreach(var property in key.Properties.Where(x=>x.Name!=nameof(IRevisionOwned.RevisionId)))property.PropertyInfo!.SetValue(entry.Copy,property.ClrType.IsValueType&&Nullable.GetUnderlyingType(property.ClrType) is null?Activator.CreateInstance(property.ClrType):null);
                navigation.SetValue(entry.Copy,target.Copy);
            } else if(key.Properties.Any(x=>x.Name!=nameof(IRevisionOwned.RevisionId)))throw new InvalidOperationException("Unsupported snapshot relationship.");
        }
        db.AddRange(copies.Select(x=>x.Copy));
    }
}
