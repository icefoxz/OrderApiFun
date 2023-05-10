using Microsoft.AspNetCore.Identity;

namespace OrderDbLib.Entities
{
    public class User : IdentityUser, IEntity
    {
        public string? Name { get; set; }
        public string? NormalizedPhoneNumber { get; set; }
        public int Version { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public long DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        public Lingau? Lingau { get; set; }

        public User()
        {
            Version = 0;
            CreatedAt = GetEpochTime();
            UpdatedAt = CreatedAt;
            DeletedAt = 0;
        }

        public long GetEpochTime()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.UtcNow - epoch).TotalSeconds;
        }

        public void UpdateFileTimeStamp()
        {
            UpdatedAt = GetEpochTime();
            Version++;
        }

        public void DeleteEntity()
        {
            IsDeleted = true;
            DeletedAt = GetEpochTime();
        }

        public void UnDelete()
        {
            IsDeleted = false;
            UpdatedAt = GetEpochTime();
            UpdateFileTimeStamp();
        }
    }
}
