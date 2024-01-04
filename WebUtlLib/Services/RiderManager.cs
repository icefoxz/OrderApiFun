using Microsoft.EntityFrameworkCore;
using OrderDbLib;
using OrderDbLib.Entities;

namespace WebUtlLib.Services;

public class RiderManager
{
    private OrderDbContext Db { get; }

    public RiderManager(OrderDbContext db)
    {
        Db = db;
    }

    public async Task<Rider?> FindByIdAsync(long? riderId, bool includeDeleted = false)
    {
        if (riderId == null) return null;
        return includeDeleted
            ? await Db.Riders.FirstOrDefaultAsync(d => d.Id == riderId)
            : await Db.Riders.Where(d => !d.IsDeleted).FirstOrDefaultAsync(d => d.Id == riderId);
    }

    public async Task<Rider> CreateRiderAsync(User user)
    {
        var man = Entity.Instance<Rider>();
        man.UserId = user.Id;
        man.IsWorking = true;
        man.User = user;
        Db.Riders.Add(man);
        await Db.SaveChangesAsync();
        return man;
    }
    public async Task<Rider> UpdateRiderAsync(Rider rider)
    {
        Db.Riders.Update(rider);
        await Db.SaveChangesAsync();
        return rider;
    }

    public async Task DeleteRiderAsync(int id)
    {
        var rider = await FindByIdAsync(id);
        if (rider != null)
        {
            rider.DeleteEntity();
            await Db.SaveChangesAsync();
        }
    }

    //用户Id查找rider
    public async Task<Rider?> FindByUserIdAsync(string userId)
    {
        if(string.IsNullOrEmpty(userId)) return null;
        return await Db.Riders.FirstOrDefaultAsync(d => !d.IsDeleted && d.UserId == userId);
    }

    public async Task<IEnumerable<Rider>> GetAllRidersAsync(bool tracking = true)
    {
        var query = Db.Riders.Include(r=>r.User).Where(d => !d.IsDeleted);
        if (tracking) query = query.AsNoTracking();
        return await query.ToListAsync();
    }
}