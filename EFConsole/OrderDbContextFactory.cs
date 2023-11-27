using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OrderDbLib;

namespace EFConsole;

public class OrderDbContextFactory : IDesignTimeDbContextFactory<OrderDbContext>
{
    public OrderDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
        //optionsBuilder.UseSqlite("Data Source=E:\\test.db");
        optionsBuilder.UseSqlServer("Server=tcp:letsmove.database.windows.net,1433;Initial Catalog=lestmovedb_t1;Persist Security Info=False;User ID=CloudSA5e76d665;Password=lestmovedb_t1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
        //optionsBuilder.UseSqlServer("Server=.;Database=t_OrderDb;User=sa;Password=123;TrustServerCertificate=True;MultipleActiveResultSets=true");

        return new OrderDbContext(optionsBuilder.Options);
    }
}