using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderDbLib;
using OrderDbLib.Entities;
using OrderHelperLib.Contracts;
using WebUtlLib;

public class RoleInitializer
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleInitializer(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task ResolveRolesAsync(string[] roles)
    {
        foreach (var roleName in roles)
        {
            if (await _roleManager.RoleExistsAsync(roleName)) continue;
            var role = new IdentityRole(roleName);
            await _roleManager.CreateAsync(role);
        }
    }
}

public class TagInitializer
{
    public static async Task InitializeSubStatesAsync(IServiceProvider sp)
    {
        // 合并所有子状态字典
        var allSubStates = DoStateMap.GetAllSubStates();
        var db = sp.GetService<OrderDbContext>();
        // 获取数据库中已存在的Tag
        var existingTags = await db.Tags
            .Where(t => t.Type == DoStateMap.TagType)
            .ToListAsync();

        var tagsToAdd = allSubStates
            .Where(subState => existingTags.All(et => et.Name != subState.StateId.ToString()))
            .Select(s=>s.ToTag()).ToArray();

        // 只添加不存在的Tag
        db.Tags.AddRange(tagsToAdd);

        await db.SaveChangesAsync();
    }
}