/// <summary>
/// The Qualifier
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum Qualifier
    {
        [EnumMember(Value = "None")]
        None,

        [EnumMember(Value = "0")]
        P0,

        [EnumMember(Value = "1")]
        P1,

        [EnumMember(Value = "2")]
        P2,

        [EnumMember(Value = "3")]
        P3,

        [EnumMember(Value = "4")]
        P4,

        [EnumMember(Value = "5")]
        P5,

        [EnumMember(Value = "6")]
        P6,

        [EnumMember(Value = "7")]
        P7,

        [EnumMember(Value = "8")]
        P8,

        [EnumMember(Value = "9")]
        P9,

        [EnumMember(Value = "10")]
        P10
    }
}