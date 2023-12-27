using System.Text;
using Microsoft.EntityFrameworkCore;
using OrderDbLib.Entities;
using Utls;

namespace DoManageWebApp.SignalHub;

public static class VersionQueryExtension
{
    //public static async Task<Dictionary<TId, int>> GetVersionsAsync<TEntity, TId>(this DbSet<TEntity> dbSet,
    //    List<TId> ids, int batchSize = 500)
    //    where TEntity : EntityBase<TId>
    //    where TId : IConvertible
    //{
    //    // 处理大量ID的策略可以在这里实现
    //    var idBatches = SplitList(ids, batchSize); // 假设每批处理1000个ID

    //    var versions = new Dictionary<TId, int>();
    //    var tableName = dbSet.EntityType.GetTableName();
    //    foreach (var batch in idBatches)
    //    {
    //        var idList = $"({string.Join(',', batch)})";
    //        var sql =
    //            $"SELECT TOP({batchSize}) [Id], [Version] FROM [{tableName}] WHERE [Id] IN {idList} AND [IsDeleted] = 0";
    //        var batchResults = await dbSet.FromSqlRaw(sql)
    //            .Select(e => new { e.Id, e.Version })
    //            .ToDictionaryAsync(entity => entity.Id, entity => entity.Version);

    //        foreach (var result in batchResults) versions.Add(result.Key, result.Value);
    //    }

    //    return versions;
    //}
    public static async Task<Dictionary<TId, int>> GetVersionsAsync<TEntity, TId>(
        this DbSet<TEntity> dbSet, List<TId> ids)
        where TEntity : EntityBase<TId>
        where TId : IConvertible
    {
        return await dbSet.Where(e => !e.IsDeleted && ids.Contains(e.Id))
                .AsNoTracking()
                .Select(e => new { e.Id, e.Version })
                .ToDictionaryAsync(e => e.Id, e => e.Version);
    }

}