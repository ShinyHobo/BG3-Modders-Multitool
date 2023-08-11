/// <summary>
/// The Penalty Qualifier
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum PenaltyQualifier
    {
        [EnumMember(Value = "None")]
        None,

        [EnumMember(Value = "-10")]
        N10,

        [EnumMember(Value = "-9")]
        N9,

        [EnumMember(Value = "-8")]
        N8,

        [EnumMember(Value = "-7")]
        N7,

        [EnumMember(Value = "-6")]
        N6,

        [EnumMember(Value = "-5")]
        N5,

        [EnumMember(Value = "-4")]
        N4,

        [EnumMember(Value = "-3")]
        N3,

        [EnumMember(Value = "-2")]
        N2,

        [EnumMember(Value = "-1")]
        N1,

        [EnumMember(Value = "0")]
        Zero,

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
        P10,

        [EnumMember(Value = "100")]
        P100
    }
}