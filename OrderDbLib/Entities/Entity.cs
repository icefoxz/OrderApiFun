using System.Collections;

namespace OrderDbLib.Entities
{
    public interface IEntity
    {
        int Version { get; set; }
        long CreatedAt { get; set; }
        long UpdatedAt { get; set; }
        long DeletedAt { get; set; }
        bool IsDeleted { get; set; }
        long GetEpochTime();
        void UpdateFileTimeStamp();
        void DeleteEntity();
        void UnDelete();
    }

    public class EntityBase<TId> : IEntity where TId : IConvertible
    {
        public EntityBase()
        {
            Version = 0;
            CreatedAt = GetEpochTime();
            UpdatedAt = CreatedAt;
            DeletedAt = 0;
        }
        public EntityBase(TId id)
        {
            Id = id;
            Version = 0;
            CreatedAt = GetEpochTime();
            UpdatedAt = CreatedAt;
            DeletedAt = 0;
        }

        public TId Id { get; set; }
        public int Version { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public long DeletedAt { get; set; }
        public bool IsDeleted { get; set; }

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

    public class Entity : EntityBase<int>
    {
        public static T Instance<T>() where T : IEntity, new()
        {
            var t = new T();
            return t;
        }
        public static T Instance<T,TId>(TId id) where T : EntityBase<TId>, new() where TId : IConvertible
        {
            var t = new T
            {
                Id = id
            };
            return t;
        }
        public Entity() : base()
        {
        }
    }
}