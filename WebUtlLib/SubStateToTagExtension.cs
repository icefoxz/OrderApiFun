using Newtonsoft.Json;
using OrderDbLib.Entities;
using OrderHelperLib.Contracts;
using Utls;

namespace WebUtlLib
{
    public static class SubStateToTagExtension
    {
        public static Tag ToTag(this DoSubState subState) => new()
        {
            Type = DoStateMap.TagType,
            Name = subState.StateId,
            Value = Json.Serialize(subState),
            Description = subState.StateName
        };
        public static DoSubState? ToSubState(this Tag tag)
        {
            if (tag == null) throw new ArgumentNullException(nameof(tag));
            if (tag.Type != DoStateMap.TagType) throw new ArgumentException("Tag type is not DoSubState");
            if (string.IsNullOrEmpty(tag.Value)) throw new ArgumentException("Tag value is null or empty");
            return Json.Deserialize<DoSubState>(tag.Value);
        }
    }
}