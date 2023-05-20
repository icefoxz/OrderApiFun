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
        void UpdateFileTimeStamp();
        void DeleteEntity();
        void UnDelete();
    }

    /// <summary>
    /// 实体父类, 主要实现:<br/>
    /// 1. 软删除 ,<br/>
    /// 2. 更新,创建 时间戳, - 用于基本的记录信息<br/>
    /// 3. 版本号 - 用于记录当前数据的纪录量和版本对比(乐观锁)<br/>
    /// 在调用构造函数的时候会有基本的数据记录
    /// </summary>
    /// <typeparam name="TId"></typeparam>
    public class EntityBase<TId> : IEntity where TId : IConvertible
    {
        /// <summary>
        /// 获取当前Unix时间戳
        /// </summary>
        /// <returns></returns>
        public static long GetEpochTime()
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(DateTime.UtcNow - epoch).TotalSeconds;
        }

        public TId Id { get; set; }
        public int Version { get; set; }
        public long CreatedAt { get; set; }
        public long UpdatedAt { get; set; }
        public long DeletedAt { get; set; }
        public bool IsDeleted { get; set; }

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

        /// <summary>
        /// 用于数据更新调用. 更新时间戳和版本号
        /// </summary>
        public void UpdateFileTimeStamp()
        {
            UpdatedAt = GetEpochTime();
            Version++;
        }

        /// <summary>
        /// 软删除调用. 更新删除时间戳和版本号
        /// </summary>
        public void DeleteEntity()
        {
            IsDeleted = true;
            DeletedAt = GetEpochTime();
        }

        /// <summary>
        /// 取消软删除调用. 更新删除时间戳和版本号
        /// </summary>
        public void UnDelete()
        {
            IsDeleted = false;
            UpdatedAt = GetEpochTime();
            UpdateFileTimeStamp();
        }
    }

    /// <summary>
    /// 实体主要生成类, 用于生成实体类
    /// </summary>
    public class Entity : EntityBase<int>
    {
        /// <summary>
        /// 基于Id(int)的实体生成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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