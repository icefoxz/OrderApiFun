
namespace OrderDbLib.Entities
{
    /// <summary>
    /// 运算服务订单
    /// </summary>
    public class DeliveryOrder : Order
    {
        //物品信息
        public ItemInfo? ItemInfo { get; set; }
        //起始地点
        public Coordinates? StartCoordinates { get; set; }
        //目的地点
        public Coordinates? EndCoordinates { get; set; }
        public string? ReceiverUserId { get; set; }
        
        // (马来西亚)州属Id
        public int MyStateId { get; set; }
        //收件人(如果有账号的话)
        public User? ReceiverUser { get; set; }
        //收件人信息
        public ReceiverInfo? ReceiverInfo { get; set; }
        //运送信息
        public DeliveryInfo? DeliveryInfo { get; set; }
        //骑手信息
        public int? RiderId { get; set; }
        public Rider? Rider { get; set; }
        //付款信息
        public PaymentInfo? PaymentInfo { get; set; }
        //订单状态
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

    /// <summary>
    /// 物品信息
    /// </summary>
    public class ItemInfo
    {
        /// <summary>
        /// 重量, 单位是kg
        /// </summary>
        public float Weight { get; set; }
        /// <summary>
        /// 物件数量, 常规来说是一个包裹. 但有时候客户会有多个物件, 这时候就需要填写数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 长, 单位是米
        /// </summary>
        public float Length { get; set; }
        /// <summary>
        /// 宽, 单位是米
        /// </summary>
        public float Width { get; set; }
        /// <summary>
        /// 高, 单位是米
        /// </summary>
        public float Height { get; set; }
        /// <summary>
        /// 附加信息
        /// </summary>
        public string? Remark { get; set; }
    }

    /// <summary>
    /// 运送信息
    /// </summary>
    public class DeliveryInfo
    {
        /// <summary>
        /// 距离, 单位是公里
        /// </summary>
        public float Distance { get; set; }
        /// <summary>
        /// 运送费
        /// </summary>
        public float Fee { get; set; }
    }
    /// <summary>
    /// 收货员信息
    /// </summary>
    public class ReceiverInfo
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 规范手机号
        /// </summary>
        public string NormalizedPhoneNumber { get; set; }
    }
    /// <summary>
    /// 坐标
    /// </summary>
    public class Coordinates
    {
        public string? PlaceId { get; set; } // 地点Id
        public string? Address { get; set; } // 地址
        public double Latitude { get; set; } // 纬度坐标
        public double Longitude { get; set; } // 经度坐标
    }
}