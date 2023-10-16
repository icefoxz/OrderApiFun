using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OrderApiFun.Core.Middlewares;
using OrderApiFun.Core.Services;
using OrderDbLib;
using OrderDbLib.Entities;
using Q_DoApi.Core.Services;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(app =>
    {
        app.UseMiddleware<AuthorityMiddleware>();
    })
    .ConfigureServices((b,s) =>
    {
        var c = b.Configuration;
        // 配置数据库连接字符串
        var connectionString = c.GetConnectionString("DefaultConnection");
        // 添加 ApplicationDbContext 服务
        s.AddDbContext<OrderDbContext>(
            op =>
                //op.UseSqlite("Data Source=E:\\test.db")
                op.UseSqlServer(connectionString)
            );

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
            .AddUserStore<UserStore<User, IdentityRole, OrderDbContext>>()
            .AddUserManager<UserManager<User>>()
            ;
        //.AddTokenProvider<JwtTokenService>(JwtTokenService.ProviderName); // 添加自定义的 JwtTokenProvider
        //.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("Default") // 添加内置的 DataProtectorTokenProvider
        // 注册 JwtTokenProvider
        s.AddSingleton<JwtTokenService>();

        // 注册必要的 Identity 服务
        s.AddDataProtection();
        s.TryAddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        s.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        s.TryAddScoped<IdentityErrorDescriber>();
        s.TryAddScoped<IUserValidator<User>, UserValidator<User>>();
        s.TryAddScoped<IPasswordValidator<User>, PasswordValidator<User>>();

        //Business Services
        s.AddScoped<DeliveryOrderService>();
        s.AddScoped<RiderManager>();
        s.AddScoped<LingauManager>();

        //Blob Storage
        var blobConnectionString = c.GetConnectionString("BlobStorage");
        var containerName = c["BlobContainerName"];
        var blobServiceClient = new BlobServiceClient(blobConnectionString);
        var blobService = new BlobService(blobServiceClient, containerName);
        s.AddSingleton(blobServiceClient);
        s.AddSingleton(blobService);
    })
    .Build();

await host.RunAsync();
