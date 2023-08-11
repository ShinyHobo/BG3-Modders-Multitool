/// <summary>
/// The DamageSourceType
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    using System.Runtime.Serialization;

    public enum DamageSourceType
    {
        BaseLevelDamage,
        AverageLevelDamge,
        MonsterWeaponDamage,
        SourceMaximumVitality,
        SourceMaximumPhysicalArmor,
        SourceMaximumMagicArmor,
        SourceCurrentVitality,
        SourceCurrentPhysicalArmor,
        SourceCurrentMagicArmor,
        SourceShieldPhysicalArmor,
        TargetMaximumVitality,
        TargetMaximumPhysicalArmor,
        TargetMaximumMagicArmor,
        TargetCurrentVitality,
        TargetCurrentPhysicalArmor,
        TargetCurrentMagicArmor,

        [EnumMember(Value = "TargetCurrentMagicArmor")]
        TargetCurrentMagicArmor2
    }
}