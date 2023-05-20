using Microsoft.AspNetCore.Identity;

namespace OrderDbLib.Entities
{
    /// <summary>
    /// 用户类, 用于登录, 以及用户的基本信息.
    /// </summary>
    public class User : IdentityUser, IEntity
    {
        public string? Name { get; set; }
        public string? NormalizedPhoneNumber { get; set; }
        public int Version { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public long DeletedAt { get; set; }
        public bool IsDeleted { get; set; }
        /// <summary>
        /// 余额信息
        /// </summary>
        public Lingau? Lingau { get; set; }

        public User()
        {
            Version = 0;
            CreatedAt = Entity.GetEpochTime();
            UpdatedAt = CreatedAt;
            DeletedAt = 0;
        }

        public void UpdateFileTimeStamp()
        {
            UpdatedAt = Entity.GetEpochTime();
            Version++;
        }

        public void DeleteEntity()
        {
            IsDeleted = true;
            DeletedAt = Entity.GetEpochTime();
        }

        public void UnDelete()
        {
            IsDeleted = false;
            UpdatedAt = Entity.GetEpochTime();
            UpdateFileTimeStamp();
        }
    }
}
