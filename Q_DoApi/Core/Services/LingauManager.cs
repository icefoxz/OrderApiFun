﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderDbLib;
using OrderDbLib.Entities;
using Q_DoApi.Core.Utls;

namespace OrderApiFun.Core.Services;

public class LingauManager
{
    private OrderDbContext Db { get; }
    public LingauManager(OrderDbContext db)
    {
        Db = db;
    }
    //更新lingau
    public async Task UpdateLingauBalanceAsync(string userId, float amount,ILogger log)
    {
        var user = await GetUserAsync(userId);
        var lastCredit = user.Lingau.Credit;
        user.Lingau.Credit += amount;
        user.Lingau.UpdateFileTimeStamp();
        user.UpdateFileTimeStamp();
        log.Event($"Amount : {amount}, User[{userId}].{lastCredit} to {user.Lingau.Credit}");
        await Db.SaveChangesAsync();
    }
    //lingau转账
    public async Task TransferLingauAsync(string fromUserId, string toUserId, float amount, ILogger log)
    {
        var fromUser = await GetUserAsync(fromUserId);
        var toUser = await GetUserAsync(toUserId);
        var fromLastCredit = fromUser.Lingau.Credit;
        var toLastCredit = toUser.Lingau.Credit;
        fromUser.Lingau.Credit -= amount;
        toUser.Lingau.Credit += amount;
        fromUser.Lingau.UpdateFileTimeStamp();
        toUser.Lingau.UpdateFileTimeStamp();
        fromUser.UpdateFileTimeStamp();
        toUser.UpdateFileTimeStamp();
        log.Event($"Amount : {amount}, From User[{fromUserId}].{fromLastCredit} to {fromUser.Lingau.Credit}, ToUser[{toUserId}] from {toLastCredit} to {toUser.Lingau.Credit}.");
        await Db.SaveChangesAsync();
    }

    public async Task<Lingau> GetLingauAsync(string userId)
    {
        var user = await GetUserAsync(userId);
        return user.Lingau;
    }
    public async Task<float> GetLingauBalanceAsync(string userId)
    {
        var lingau = await GetLingauAsync(userId);
        return lingau.Credit;
    }
    private async Task<User?> GetUserAsync(string userId)
    {
        return await Db.Users.Include(u => u.Lingau).FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

}