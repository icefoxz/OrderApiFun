using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderDbLib;
using OrderHelperLib.Contracts;
using Syncfusion.Licensing;
using WebUtlLib;

public static class AppInitializer
{
    public static async Task InitializeAsync(WebApplication app)
    {
        await InitializeSyncfusionAsync(app.Configuration);
        await ResolveRolesAsync([Auth.Role_User, Auth.Role_Rider], app.Services);
        await InitializeSubStatesAsync(app.Services);
    }

    private static Task InitializeSyncfusionAsync(IConfiguration config)
    {
        var license = config["SyncfusionLicense"];
        SyncfusionLicenseProvider.RegisterLicense(license);
        return Task.CompletedTask;
    }

    private static async Task ResolveRolesAsync(string[] roles, IServiceProvider sp)
    {
        using var roleManager = sp.CreateAsyncScope().ServiceProvider.GetService<RoleManager<IdentityRole>>();
        foreach (var roleName in roles)
        {
            if (await roleManager.RoleExistsAsync(roleName)) continue;
            var role = new IdentityRole(roleName);
            await roleManager.CreateAsync(role);
        }
    }

    private static async Task InitializeSubStatesAsync(IServiceProvider sp)
    {
        // 合并所有子状态字典
        var allSubStates = DoStateMap.GetAllSubStates().ToArray();
        await using var db = sp.CreateAsyncScope().ServiceProvider.GetService<OrderDbContext>();
        // 获取数据库中已存在的Tag
        var existingTags = await db.Tags
            .Where(t => t.Type == DoStateMap.TagType && !t.IsDeleted)
            .ToListAsync();

        var unusedTags = existingTags.ToList();
            //allSubStates
            //.Where(subState => existingTags.All(et => et.Name != subState.StateId.ToString()))
            //.Select(s=>s.ToTag()).ToArray();
        foreach (var subState in allSubStates)
        {
            var ex = existingTags.FirstOrDefault(et => et.Name == subState.StateId);
            if (ex == null)
            {
                db.Tags.Add(subState.ToTag());
                continue;
            }
            existingTags.Remove(ex);
            unusedTags.Remove(ex);
            var updateTag = subState.ToTag();
            if (ex != updateTag)
            {
                ex.Set(updateTag);
                db.Tags.Update(ex);
            }
        }

        foreach (var tag in unusedTags)
        {
            tag.DeleteEntity();
            db.Tags.Update(tag);
        }

        await db.SaveChangesAsync();
    }
}