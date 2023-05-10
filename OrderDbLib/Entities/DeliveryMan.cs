namespace OrderDbLib.Entities
{
    public class DeliveryMan : Entity
    {
        public string? Location { get; set; }
        public bool IsWorking { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}