/// <summary>
/// Passive flags.
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    public enum PassiveFlag
    {
        None,
        OncePerTurn,
        ExecuteOnce,
        IsHidden,
        IsToggled,
        ToggledDefaultOn,
        ToggledDefaultAddToHotbar,
        OncePerCombat,
        OncePerShortRest,
        OncePerLongRest,
        MetaMagic,
        OncePerAttack,
        Highlighted,
        Temporary,
        OncePerShortRestPerItem,
        OncePerLongRestPerItem,
        ToggleForParty,
        ForceShowInCC,
        DisplayBoostInTooltip,
    }
}