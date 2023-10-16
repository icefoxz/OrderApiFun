using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

        public OrderDbContext(DbContextOptions op):base(op) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
            //b.Entity<DeliveryOrder>().Property(d => d.EndCoordinates).HasConversion(o => JsonConvert.SerializeObject(o), o => JsonConvert.DeserializeObject<Coordinates>(o));
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
            b.Entity<DeliveryOrder>().OwnsOne(d => d.PaymentInfo);
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
    }
}

