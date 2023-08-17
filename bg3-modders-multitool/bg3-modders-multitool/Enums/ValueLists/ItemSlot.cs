/// <summary>
/// The item slot types.
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum ItemSlot
    {
        Helmet,
        Breast,
        Cloak,

        [EnumMember(Value = "Melee Main Weapon")]
        MeleeMainWeapon,

        [EnumMember(Value = "Melee Offhand Weapon")]
        MeleeOffhandWeapon,

        [EnumMember(Value = "Ranged Main Weapon")]
        RangedMainWeapon,

        [EnumMember(Value = "Ranged Offhand Weapon")]
        RangedOffhandWeapon,

        Ring,
        Belt,
        Boots,
        Gloves,
        Amulet,
        Ring2,
        Wings,
        Horns,
        Overhead,
        MusicalInstrument,
        VanityBody,
        VanityBoots,
        Underwear
    }
}