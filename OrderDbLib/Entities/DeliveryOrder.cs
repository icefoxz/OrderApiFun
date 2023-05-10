
namespace OrderDbLib.Entities
{
    /// <summary>
    /// 运算服务订单
    /// </summary>
    public class DeliveryOrder : Order
    {
        public ItemInfo? ItemInfo { get; set; }
        public Coordinates? StartCoordinates { get; set; }
        public Coordinates? EndCoordinates { get; set; }
        public string? ReceiverUserId { get; set; }
        public User? ReceiverUser { get; set; }
        public ReceiverInfo? ReceiverInfo { get; set; }
        public DeliveryInfo? DeliveryInfo { get; set; }
        public int? RiderId { get; set; }
        public Rider? Rider { get; set; }
        public PaymentInfo? PaymentInfo { get; set; }
        public int Status { get; set; }
    }

    public class PaymentInfo
    {
        public float Price { get; set; } // 价格
        public int PaymentMethod { get; set; } // 付款类型
        /// <summary>
        /// 付款Reference,如果骑手代收将会是骑手Id, 如果是在线支付将会是支付平台的Reference, 如果是用户扣账将会是用户Id
        /// </summary>
        public string? PaymentReference { get; set; } 
        public bool PaymentReceived { get; set; } // 是否已经完成付款
    }

    public class ItemInfo
    {
        public float Weight { get; set; }
        public int Quantity { get; set; }
        public float Length { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public string? Remark { get; set; }
    }

    public class DeliveryInfo
    {
        public float Distance { get; set; }
        public float Weight { get; set; }
        public float Price { get; set; }
    }
    public class ReceiverInfo
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string NormalizedPhoneNumber { get; set; }
    }
    /// <summary>
    /// 坐标
    /// </summary>
    public class Coordinates
    {
        public string? Address { get; set; } // 地址
        public double Latitude { get; set; } // 纬度坐标
        public double Longitude { get; set; } // 经度坐标
    }
}