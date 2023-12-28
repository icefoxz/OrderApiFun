using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var host = new HostBuilder()
    //.ConfigureFunctionsWebApplication(app =>
    //{
    //    app.UseMiddleware<AuthorityMiddleware>();
    //})
    .ConfigureServices((b, s) =>
    {
        var c = b.Configuration;

        // 配置数据库连接字符串
        //var connectionString = c.GetConnectionString(Config.DefaultConnectionString);
        //// 添加 ApplicationDbContext 服务
        //s.AddDbContext<OrderDbContext>(
        //    op =>
        //        //op.UseSqlite("Data Source=E:\\test.db")
        //        op.UseSqlServer(connectionString)
        //    );

        //var signalRServerUrl = c[Config.SignalRServerUrl];
        //SignalRCall.Init(signalRServerUrl);

        //Middleware
        //s.AddHttpContextAccessor();//Middleware support MUST!
        // 添加 Identity 服务
        //s.AddIdentity<User, IdentityRole>(o =>
        //{
        //    o.Password.RequireDigit = false;
        //    o.Password.RequiredLength = 5;
        //    o.Password.RequireLowercase = false;
        //    o.Password.RequireUppercase = false;
        //    o.Password.RequireNonAlphanumeric = false;
        //    o.Password.RequiredUniqueChars = 4;
        //})
        //    .AddEntityFrameworkStores<OrderDbContext>()
        //    //.AddUserStore<UserStore<User, IdentityRole, OrderDbContext>>()
        //    //.AddUserManager<UserManager<User>>()
        //    //.AddRoleManager<RoleManager<IdentityRole>>()
        //    ;
        //
        //s.AddDataProtection();
        //s.AddSingleton<RoleInitializer>();
        //.AddTokenProvider<JwtTokenService>(JwtTokenService.ProviderName); // 添加自定义的 JwtTokenProvider
        //.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("Default") // 添加内置的 DataProtectorTokenProvider
        // 注册 JwtTokenProvider
        //s.AddScoped<JwtTokenService>();

        //Business Services
        //s.AddScoped<DoService>();
        //s.AddScoped<RiderManager>();
        //s.AddScoped<LingauManager>();

        //Blob Storage
        var blobConnectionString = c.GetConnectionString("BlobStorage");
        var containerName = c["BlobContainerName"];
        //var blobServiceClient = new BlobServiceClient(blobConnectionString);
        //var blobService = new BlobService(blobServiceClient, containerName);
        //s.AddSingleton(blobServiceClient);
        //s.AddSingleton(blobService);
    })
.Build();
//var roleInitializer = host.Services.GetService<RoleInitializer>();
//string[] roles = { Auth.Role_User, Auth.Role_Rider };
//await roleInitializer.ResolveRolesAsync(roles: roles);
await TagInitializer.InitializeSubStatesAsync(host.Services);
await host.RunAsync();

public class Config
{
    public const string DefaultConnectionString = "DefaultConnection";
    public const string SignalRServerUrl = "SignalRServerUrl";
}