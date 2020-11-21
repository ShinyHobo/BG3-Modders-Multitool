/// <summary>
/// The spell flag list.
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    public enum SpellFlagList
    {
        None,
        HasVerbalComponent,
        HasSomaticComponent,
        IsJump,
        IsAttack,
        IsMelee,
        HasHighGroundRangeExtension,
        IsConcentration,
        AddFallDamageOnLand,
        ConcentrationIgnoresResting,
        DefaultThrow,
        IsSpell,
        ForGameMaster,
        IsEnemySpell,
        CannotTargetCharacter,
        CannotTargetItems,
        CannotTargetTerrain,
        IgnoreVisionBlock,
        Stealth,
        AddWeaponRange,
        IgnoreSilence,
        ImmediateCast,
        RangeIgnoreSourceBounds,
        RangeIgnoreTargetBounds,
        RangeIgnoreVerticalThreshold,
        NoSurprise,
        IsHarmful,
        IsTrap,
        IsDefaultWeaponAction,
        IsRequiredShield,
        TargetClosestEqualGroundSurface,
        CannotRotate,
        DontForceSheathOrUnsheath,
        CanDualWield,
        IsLinkedSpellContainer
    }
}
