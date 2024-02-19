/// <summary>
/// The Handedness
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum Handedness
    {
        [EnumMember(Value = "Any")]
        Any,

        [EnumMember(Value = "1")]
        One,

        [EnumMember(Value = "2")]
        Two
    }
}