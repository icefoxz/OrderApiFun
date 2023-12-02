using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderDbLib.Entities;

namespace OrderDbLib
{
    public class OrderDbContext: IdentityDbContext<User>
    {
        public DbSet<DeliveryOrder> DeliveryOrders { get; set; }
        public DbSet<Rider> Riders { get; set; }
        public DbSet<Lingau> Lingaus { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Tag_Do> Tag_dos { get; set; }
        public DbSet<Tag_Report> Tag_reports{ get; set; }

        public OrderDbContext(DbContextOptions op):base(op) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            //b.Entity<DeliveryOrder>().Property(d => d.EndCoordinates)
            //.HasConversion(o => JsonConvert.SerializeObject(o), o => JsonConvert.DeserializeObject<Coordinates>(o));
            // 配置多对多关系
            // 定义复合主键
            ConfigManyToMany(b);
            ConfigDeliveryOrder(b);

            b.Entity<User>().HasOne<Lingau>()
                .WithOne()
                .HasForeignKey<Lingau>(l => l.UserRefId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Entity<Lingau>()
                .Property(l => l.Id)
                .HasDefaultValueSql("NEWID()");
            b.Entity<Report>().OwnsOne(r => r.Resolve);
            base.OnModelCreating(b);
        }

        // DeliveryOrder配置
        private static void ConfigDeliveryOrder(ModelBuilder b)
        {
            b.Entity<DeliveryOrder>().HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            b.Entity<DeliveryOrder>().OwnsOne(d => d.ItemInfo);
            b.Entity<DeliveryOrder>().OwnsOne(d => d.ReceiverInfo);
            b.Entity<DeliveryOrder>().OwnsOne(d => d.SenderInfo);
            b.Entity<DeliveryOrder>().OwnsOne(d => d.DeliveryInfo, i =>
            {
                i.OwnsOne(o => o.StartLocation);
                i.OwnsOne(o => o.EndLocation);
            });
            b.Entity<DeliveryOrder>().Property(d => d.StateHistory)
                .HasConversion(h => JsonConvert.SerializeObject(h),
                    s => JsonConvert.DeserializeObject<StateSegment[]>(s) ?? Array.Empty<StateSegment>());
            b.Entity<DeliveryOrder>().OwnsOne(d => d.PaymentInfo);
        }

        // 多对多关系配置
        private static void ConfigManyToMany(ModelBuilder b)
        {
            b.Entity<Tag_Do>().HasKey(t => new { t.DeliveryOrderId, t.TagId });
            b.Entity<Tag_Do>()
                .HasOne(t => t.DeliveryOrder)
                .WithMany(d => d.Tag_Dos)
                .HasForeignKey(t => t.DeliveryOrderId);
            // 定义与 Tag 的关系
            b.Entity<Tag_Do>()
                .HasOne(t => t.Tag)
                .WithMany() // 不指定导航属性
                .HasForeignKey(t => t.TagId);

            b.Entity<Tag_Report>().HasKey(t => new { t.ReportId, t.TagId });
            b.Entity<Tag_Report>()
                .HasOne(t => t.Report)
                .WithMany(d => d.Tag_Reports)
                .HasForeignKey(t => t.ReportId);
            // 定义与 Tag 的关系
            b.Entity<Tag_Report>()
                .HasOne(t => t.Tag)
                .WithMany() // 不指定导航属性
                .HasForeignKey(t => t.TagId);
        }


        //重写SaveChangesAsync方法并地中调用UpdateFileTimeStamp()
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            // 获取所有已更改的实体
            var modifiedEntries = ChangeTracker.Entries()
                .Where(entry => entry.State == EntityState.Modified);

            foreach (var entry in modifiedEntries)
            {
                if (entry.Entity is EntityBase<IConvertible> entityBase)
                {
                    // 调用UpdateFileTimeStamp()方法
                    entityBase.UpdateFileTimeStamp();
                }
            }
            // 调用基类的SaveChangesAsync来实际保存更改
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}