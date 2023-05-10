using Microsoft.EntityFrameworkCore;
using OrderDbLib;
using OrderDbLib.Entities;

namespace OrderApiFun.Core.Services;

public class RiderManager
{
    private OrderDbContext Db { get; }

    public RiderManager(OrderDbContext db)
    {
        Db = db;
    }

    public async Task<Rider?> FindByIdAsync(int deliveryManId, bool includeDeleted = false) => includeDeleted
        ? await Db.Riders.FirstOrDefaultAsync(d => d.Id == deliveryManId)
        : await Db.Riders.Where(d => !d.IsDeleted).FirstOrDefaultAsync(d => d.Id == deliveryManId);

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
    public async Task<Rider?> FindByUserIdAsync(string userId) =>
        await Db.Riders.FirstOrDefaultAsync(d => !d.IsDeleted && d.UserId == userId);
    //用riderId查找rider
    public async Task<Rider?> FindByRiderIdAsync(int riderId) =>
        await Db.Riders.FirstOrDefaultAsync(d => !d.IsDeleted && d.Id == riderId);
}