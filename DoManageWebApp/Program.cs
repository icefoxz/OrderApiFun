using DoManageWebApp.Components;
using DoManageWebApp.Components.Account;
using DoManageWebApp.SignalHub;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using OrderApiFun.Core.Services;
using OrderDbLib;
using OrderDbLib.Entities;
using Syncfusion.Blazor;
using Syncfusion.Licensing;
using WebUtlLib;
using WebUtlLib.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//Syncfusion
builder.Services.AddSyncfusionBlazor();

//注册服务器用于http请求的client
builder.Services.AddHttpClient();

//SignalR
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
builder.Services.AddScoped<ServerCallService>();
builder.Services.AddScoped<SignalRCallService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<OrderCallSignalRClientService>();

//Controller
builder.Services.AddControllers();

//Services
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<RiderManager>();

//Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<OrderDbContext>(op =>
    op.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Identity
builder.Services.AddIdentityCore<User>(op =>
    {
        op.SignIn.RequireConfirmedAccount = false;
        op.Password.RequireDigit = false;
        op.Password.RequireLowercase = false;
        op.Password.RequireNonAlphanumeric = false;
        op.Password.RequireUppercase = false;
        op.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddSignInManager()
    .AddEntityFrameworkStores<OrderDbContext>()
    .AddDefaultTokenProviders();

//Authentication
builder.Services.AddAuthentication(op =>
    {
        op.DefaultScheme = IdentityConstants.ApplicationScheme;
        op.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(op =>
    {
        op.TokenValidationParameters = JwtTokenService.TokenValidationParameters;
        op.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/orderhub"))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddIdentityCookies();

//Email Sender
builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();


/*********************App *********************/
var app = builder.Build();

await AppInitializer.InitializeAsync(app);

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapHub<OrderHub>("/orderhub");

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
