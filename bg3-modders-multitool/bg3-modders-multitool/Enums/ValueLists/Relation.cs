/// <summary>
/// The Relation
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    using System.Runtime.Serialization;

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