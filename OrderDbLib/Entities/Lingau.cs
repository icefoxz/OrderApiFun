namespace OrderDbLib.Entities;

/// <summary>
/// 令凹币
/// </summary>
public class Lingau : EntityBase<string>
{
    /// <summary>
    /// 余额
    /// </summary>
    public float Credit { get; set; }
    /// <summary>
    /// 用户外键
    /// </summary>
    public string? UserRefId { get; set; }
}