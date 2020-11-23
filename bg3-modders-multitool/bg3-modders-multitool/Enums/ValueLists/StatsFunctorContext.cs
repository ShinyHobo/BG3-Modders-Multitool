/// <summary>
/// The stats functor context.
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    public enum StatsFunctorContext
    {
        None,
        Target,
        AOE,
        OnCast,
        OnEquip,
        Ground,
        OnLeaveAttackRange,
        OnEntityAttackedWithinMeleeRange,
        OnEntityAttackingWithinMeleeRange,
        OnProficiencyChange,
        OnStatusRemoveTimeOut,
        OnStatusRemoveConditions,
        OnStatusRemoveExternal,
        OnMoveStartedInCombat,
        AiOnly,
        AiIgnore,
        OnAttack,
        OnAttacked,
        OnDamage,
        OnHeal,
        OnStatusRemoveDeath,
        OnObscurityChanged,
        OnShortRest
    }
}
