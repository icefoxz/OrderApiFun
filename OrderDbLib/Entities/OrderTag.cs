namespace OrderDbLib.Entities
{
    /// <summary>
    /// OrderTags（订单标签）
    /// 主要作是为<see cref="Order"/>标签形式表述当前<see cref="Order"/>的一种状况.
    /// </summary>
    public class OrderTag : Entity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}