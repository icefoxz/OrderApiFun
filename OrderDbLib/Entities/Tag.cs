namespace OrderDbLib.Entities
{
    /// <summary>
    /// Tag（标签）
    /// </summary>
    //public class OrderTag : Tag
    //{
    //    public override string Type { get; set; } = "OrderTag";
    //}

    //public class ReportTag : Tag
    //{
    //    public override string Type { get; set; } = "ReportTag";
    //}

    public class Tag : Entity
    {
        public string Type { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class Tag_Do : Entity
    {
        public long DeliveryOrderId { get; set; }
        public DeliveryOrder DeliveryOrder { get; set; }
        public long TagId { get; set; }
        public Tag Tag { get; set; }
    }    

    public class Tag_Report : Entity
    {
        public long ReportId { get; set; }
        public Report Report { get; set; }
        public long TagId { get; set; }
        public Tag Tag { get; set; }
    }
}