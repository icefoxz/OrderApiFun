using DoManageWebApp.Components;
using DoManageWebApp.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderDbLib;
using OrderDbLib.Entities;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//Syncfusion
builder.Services.AddSyncfusionBlazor();
//注册服务器用于http请求的client
builder.Services.AddHttpClient();

//Services
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

//Authentication
builder.Services.AddAuthentication(op =>
    {
        op.DefaultScheme = IdentityConstants.ApplicationScheme;
        op.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

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
    .AddEntityFrameworkStores<OrderDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

//Email Sender
builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();


/*********************App *********************/
var app = builder.Build();

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
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
