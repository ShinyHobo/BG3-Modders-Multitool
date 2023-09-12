/// <summary>
/// The interupt context.
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    public enum InterruptContext
    {
        None,
        OnSpellCast,
        OnPostRoll,
        OnCastHit,
        OnPreDamage,
        OnLeaveAttackRange,
        OnEnterAttackRange
    }
}