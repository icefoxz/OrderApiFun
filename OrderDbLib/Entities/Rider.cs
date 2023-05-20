namespace OrderDbLib.Entities
{
    public class Rider : Entity
    {
        public string? Name { get; set; } 
        public string? Phone { get; set; }
        public string? Location { get; set; }
        public bool IsWorking { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }
    }
}