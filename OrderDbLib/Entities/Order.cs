
namespace OrderDbLib.Entities
{
    /// <summary>
    /// 订单的基础类, 主要已线程方式实现各种状态<see cref="OrderTag"/>
    /// </summary>
    public class Order : Entity
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public ICollection<OrderTag> Tags { get; set; } = new HashSet<OrderTag>();
    }
}