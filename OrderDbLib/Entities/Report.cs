namespace OrderDbLib.Entities;

public class Report : Entity
{
    public DeliveryOrder Order { get; set; }
    public int OrderId { get; set; }
    public long TimeOfOccurrence { get; set; }
    public ICollection<Tag> Tags { get; set; }
    /// <summary>
    /// 事件描述
    /// </summary>
    public string? IncidentDescription { get; set; }
    /// <summary>
    /// 影响描述(内部)
    /// </summary>
    public string? ImpactDescription { get; set; }
    public ReportResolve? Resolve { get; set; }
    /// <summary>
    /// 补充
    /// </summary>
    public string? Remark { get; set; }
}

public class ReportResolve
{
    /// <summary>
    /// 解决描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 解决后的订单状态, Cancel or Close
    /// </summary>
    public int OrderStatus { get; set; }
    /// <summary>
    /// 解决后的付款状态, Refund = 1 or None = 0
    /// </summary>
    public int PaymentStatus { get; set; }
}