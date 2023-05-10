// See https://aka.ms/new-console-template for more information

using EFConsole;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderHelperLib.Utl;

Console.WriteLine("Hello, World!");

#region Db
var builder = new OrderDbContextFactory();
var db = builder.CreateDbContext(Array.Empty<string>());
db.Users.Load();
foreach (var dbUser in db.Users)
{
    Console.WriteLine($"{dbUser.UserName}");
}
#endregion

var response = await Http.PostAsync("http://localhost:7236/api/Anonymous_LoginApi",
    JsonConvert.SerializeObject(new { username = "ta247", password = "Xa@123456" }));
var result = await response.Content.ReadAsStringAsync();
Console.WriteLine(result);
var model = JsonConvert.DeserializeObject<LoginResponse>(result);
var accessClient = Http.InstanceAccessClient(model.Token);
var re = await accessClient.PostAsync("http://localhost:7236/api/User_TestApi", string.Empty);
var r1 = await re.Content.ReadAsStringAsync();
Console.WriteLine(r1);

class LoginResponse
{
    public string Token { get; set; }
    public string Refresh { get; set; }
}