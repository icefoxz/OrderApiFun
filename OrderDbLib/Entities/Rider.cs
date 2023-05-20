namespace OrderDbLib.Entities
{
    /// <summary>
    /// 骑手类, 是运送单位.
    /// </summary>
    public class Rider : Entity
    {
        /// <summary>
        /// 名字
        /// </summary>
        public string? Name { get; set; } 
        /// <summary>
        /// 手机
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// 是否工作状态, 例如: 休息, 工作中, 离职
        /// </summary>
        public bool IsWorking { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }
    }
}