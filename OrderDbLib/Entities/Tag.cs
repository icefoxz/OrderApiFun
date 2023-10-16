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
        public string Type { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}