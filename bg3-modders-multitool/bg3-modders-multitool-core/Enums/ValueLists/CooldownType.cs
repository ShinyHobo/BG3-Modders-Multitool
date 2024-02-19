/// <summary>
/// The cooldown types.
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    public enum CooldownType
    {
        None,
        OncePerTurn,
        OncePerCombat,
        OncePerRest,
        OncePerTurnNoRealtime,
        OncePerShortRest,
        OncePerRestPerItem,
        OnCombatEnded,
        OncePerShortRestPerItem
    }
}