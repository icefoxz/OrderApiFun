
namespace OrderDbLib.Entities
{
    /// <summary>
    /// 运算服务订单
    /// </summary>
    public class DeliveryOrder : Entity
    {
        // 执行用户Id
        public string UserId { get; set; }
        // 执行用户
        public User User { get; set; }
        //物品信息
        public ItemInfo ItemInfo { get; set; }
        // (马来西亚)州属Id
        public string MyState { get; set; }
        //寄件人信息
        public SenderInfo SenderInfo { get; set; }
        //收件人信息
        public ReceiverInfo ReceiverInfo { get; set; }
        //运送信息
        public DeliveryInfo DeliveryInfo { get; set; }
        //骑手信息
        public long? RiderId { get; set; }
        public Rider? Rider { get; set; }
        //付款信息
        public PaymentInfo? PaymentInfo { get; set; }
        //订单状态, 正数 = 进行中, 负数 = 已完成
        public int Status { get; set; }
        //订单子状态
        public int SubState { get; set; }
        //订单状态历史(进程)
        public StateSegment[] StateHistory { get; set; } = Array.Empty<StateSegment>();
        //标签, 于订单状态进程相比, 这个是用来分析和过滤的
        public ICollection<Tag_Do> Tag_Dos { get; set; }
        //订单报告
        public ICollection<Report> Reports { get; set; }
    }

    //订单状态进程, 用于记录订单的状态变化
    public class StateSegment
    {
        public int SubState { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Remark { get; set; }
    }

    public class PaymentInfo
    {
        /// <summary>
        /// 运送费
        /// </summary>
        public float Fee { get; set; }
        public float Charge { get; set; } // 价格
        public string Method { get; set; } = string.Empty; // 付款类型
        /// <summary>
        /// 付款Reference,如果骑手代收将会是骑手Id, 如果是在线支付将会是支付平台的Reference, 如果是用户扣账将会是用户Id
        /// </summary>
        public string? Reference { get; set; }
        /// <summary>
        /// 付款TransactionId
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;
        public bool IsReceived { get; set; } // 是否已经完成付款
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
        /// 材积, 长*宽*高/6000
        /// </summary>
        public double Volume { get; set; }
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
        //出发地点
        public Location StartLocation { get; set; }
        //目的地点
        public Location EndLocation { get; set; }
        /// <summary>
        /// 距离, 单位是公里
        /// </summary>
        public float Distance { get; set; }
    }
    /// <summary>
    /// 发货员信息
    /// </summary>
    public class SenderInfo
    {
        // 发件人Id(如果有账号的话))
        public string? UserId { get; set; }
        public User? User { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string NormalizedPhoneNumber { get; set; }
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
        // 收件人Id(如果有账号的话))
        public string? UserId { get; set; }
        //收件人(如果有账号的话)
        public User? User { get; set; }
    }
    /// <summary>
    /// 地点
    /// </summary>
    public class Location
    {
        public string? PlaceId { get; set; } // 地点Id
        public string? Address { get; set; } // 地址
        public double Latitude { get; set; } // 纬度坐标
        public double Longitude { get; set; } // 经度坐标
    }
}