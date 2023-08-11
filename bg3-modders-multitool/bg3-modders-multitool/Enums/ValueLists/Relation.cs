using System.Runtime.Serialization;

/// <summary>
/// The Relation
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    [DataContract]
    public enum Relation
    {
        [EnumMember(Value = "Ally")]
        Ally,

        [EnumMember(Value = "Neutral")]
        Neutral,

        [EnumMember(Value = "Enemy")]
        Enemy,

        [EnumMember(Value = "Persistent Neutral")]
        PersistentNeutral
    }
}