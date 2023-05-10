namespace OrderDbLib.Entities
{
    /// <summary>
    /// OrderTags（订单标签）
    /// 主要是作为<see cref="DeliveryOrder"/>处理的一个"状态", 将结合框架下对应的功能执行.
    /// </summary>
    public class OrderTag : Entity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}