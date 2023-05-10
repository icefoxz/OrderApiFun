namespace OrderDbLib.Entities;

public class Lingau : EntityBase<string>
{
    public float Credit { get; set; }
    public string? UserRefId { get; set; }
}