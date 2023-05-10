namespace OrderModelLib.Entities
{
    public class User : EntityBase<string>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
