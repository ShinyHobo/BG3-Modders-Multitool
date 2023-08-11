using System.Runtime.Serialization;

/// <summary>
/// The DamageSourceType
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
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