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
        OnStatusApply,
        OnStatusApplied,
        OnStatusRemove,
        OnMovedDistance,
        AiOnly,
        AiIgnore,
        OnAttack,
        OnAttacked,
        OnDamage,
        OnHeal,
        OnStatusRemoved,
        OnObscurityChanged,
        OnShortRest,
        OnDamaged,
        OnHealed,
        OnAbilityCheck,
        OnCastResolved,
        OnLongRest,
        OnCreate,
        OnPush,
        OnPushed,
        OnInventoryChanged,
        OnEnterAttackRange,
        OnProjectileExploded,
        OnCombatEnded,
        OnTurn,
        OnActionResourcesChanged,
        OnSurfaceEnter,
        OnDamagedPrevented,
        OnInterruptUsed
    }
}