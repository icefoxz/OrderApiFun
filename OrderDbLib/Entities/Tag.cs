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

    public class Tag : Entity, IEquatable<Tag>
    {
        public string Type { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

        public bool Equals(Tag? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && Value == other.Value && Name == other.Name && Description == other.Description;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Tag)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type.GetHashCode();
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Tag? left, Tag? right) => Equals(left, right);
        public static bool operator !=(Tag? left, Tag? right) => !Equals(left, right);

        public void Set(Tag tag)
        {
            Type = tag.Type;
            Value = tag.Value;
            Name = tag.Name;
            Description = tag.Description;
        }
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