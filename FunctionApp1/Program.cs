using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderApiFun.Core.Middlewares;
using OrderApiFun.Core.Services;
using OrderDbLib;
using OrderDbLib.Entities;
using Q_DoApi.Core.Services;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(app=>app.UseMiddleware<AuthorityMiddleware>())
    .ConfigureServices((b,s) =>
    {
        var c = b.Configuration;

        // 配置数据库连接字符串
        var connectionString = c.GetConnectionString(Config.DefaultConnectionString);
        // 添加 ApplicationDbContext 服务
        s.AddDbContext<OrderDbContext>(
            op => op.UseSqlServer(connectionString));

        var signalRServerUrl = c[Config.SignalRServerUrl];
        SignalRCall.Init(signalRServerUrl);

        //Middleware
        //s.AddHttpContextAccessor();//Middleware support MUST!
        // 添加 Identity 服务
        s.AddIdentityCore<User>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequiredLength = 5;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredUniqueChars = 4;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<OrderDbContext>();

        s.AddSingleton<RoleInitializer>();
        // 注册 JwtTokenProvider
        s.AddScoped<JwtTokenService>();

        //Business Services
        s.AddScoped<DoService>();
        s.AddScoped<RiderManager>();
        s.AddScoped<LingauManager>();

        //Blob Storage
        s.AddScoped<IBlobService, BlobService>();
    })
.Build();
//var roleInitializer = host.Services.GetService<RoleInitializer>();
//string[] roles = { Auth.Role_User, Auth.Role_Rider };
//await roleInitializer.ResolveRolesAsync(roles: roles);
//await TagInitializer.InitializeSubStatesAsync(host.Services);
await host.RunAsync();

public class Config
{
    public const string DefaultConnectionString = "DefaultConnection";
    public const string SignalRServerUrl = "SignalRServerUrl";
}