using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderApiFun.Core.Middlewares;
using OrderApiFun.Core.Services;
using OrderDbLib;
using OrderDbLib.Entities;
using Q_DoApi.Core.Services;

var builder = new HostBuilder()
    .ConfigureFunctionsWebApplication(app => app.UseMiddleware<AuthorityMiddleware>())
    .ConfigureServices((b,s) =>
    {
        var c = b.Configuration;
        Config.Init(c);
        // 配置数据库连接字符串
        var connectionString = Config.DbConnectionString();
        // 添加 ApplicationDbContext 服务
        s.AddDbContext<OrderDbContext>(
            op =>
                //op.UseSqlite("Data Source=E:\\test.db")
                op.UseSqlServer(connectionString)
            );

        //Middleware
        //s.AddHttpContextAccessor();//Middleware support MUST!
        // 添加 Identity 服务
        s.AddIdentity<User,IdentityRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequiredLength = 5;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredUniqueChars = 4;
            })
            .AddEntityFrameworkStores<OrderDbContext>()
            //.AddUserStore<UserStore<User, IdentityRole, OrderDbContext>>()
            //.AddUserManager<UserManager<User>>()
            //.AddRoleManager<RoleManager<IdentityRole>>()
            ;

        s.AddDataProtection();
        s.AddSingleton<RoleInitializer>();
        //.AddTokenProvider<JwtTokenService>(JwtTokenService.ProviderName); // 添加自定义的 JwtTokenProvider
        //.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("Default") // 添加内置的 DataProtectorTokenProvider
        // 注册 JwtTokenProvider
        s.AddScoped<JwtTokenService>();

        //Business Services
        s.AddScoped<DoService>();
        s.AddScoped<RiderManager>();
        s.AddScoped<LingauManager>();
        s.AddScoped<SignalRCall>();

        //Blob Storage
        s.AddScoped<BlobService>();
    });

var host = builder.Build();

//var roleInitializer = host.Services.GetService<RoleInitializer>();
//string[] roles = { Auth.Role_User, Auth.Role_Rider };
//await roleInitializer.ResolveRolesAsync(roles: roles);
//await TagInitializer.InitializeSubStatesAsync(host.Services);
await host.RunAsync();

public static class Config
{
    private static IConfiguration _config;
    public static void Init(IConfiguration configuration)
    {
        _config = configuration;
    }

    public static string Get(string key)
    {
        var value = _config[key];
        return value ?? throw new NotImplementedException($"no value: {key}");
    }

    //ConnectionStrings
    private const string DefaultConnectionString = "DefaultConnection";
    private const string BlobStorageConnectionString = "BlobStorage";
    private static string GetConnectionString(string key) => _config.GetConnectionString(key) ?? throw new NotImplementedException($"no connectionString: {key}");
    public static string DbConnectionString() => GetConnectionString(DefaultConnectionString);
    public static string BlobConnectionString() => GetConnectionString(BlobStorageConnectionString);

    //Values
    private const string SignalRServerUrl = "SignalRServerUrl";
    public static string GetSignalRServerUrl() => Get(SignalRServerUrl);
    private const string OrderHub = "OrderHub";
    public static string GetSignalRHubUrl() => $"{GetSignalRServerUrl()}/{OrderHub}";
    private const string BlobContainerName = "BlobContainerName";
    public static string GetBlobContainerName() => Get(BlobContainerName);

}