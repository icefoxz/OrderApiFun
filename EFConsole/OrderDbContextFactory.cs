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
        optionsBuilder.UseSqlServer("Server=.;Database=PoDb;User=sa;Password=123;TrustServerCertificate=True;MultipleActiveResultSets=true");

        return new OrderDbContext(optionsBuilder.Options);
    }
}