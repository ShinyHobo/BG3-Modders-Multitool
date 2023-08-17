/// <summary>
/// The status events.
/// </summary>
namespace bg3_modders_multitool.Enums.ValueLists
{
    public enum StatusEvent
    {
        None,
        OnTurn,
        OnSpellCast,
        OnAttack,
        OnAttacked,
        OnApply,
        OnRemove,
        OnApplyAndTurn,
        OnDamage,
        OnEquip,
        OnUnequip,
        OnHeal,
        OnObscurityChanged,
        OnSightRelationsChanged,
        OnSurfaceEnter,
        OnStatusApplied,
        OnStatusRemoved,
        OnMove,
        OnCombatEnded,
        OnRemovePerformanceRequest,
        OnLockpickingSucceeded,
        OnSourceDeath,
        OnSourceStatusApplied,
        OnFactionChanged,
        OnEntityPickUp,
        OnEntityDrop,
        OnEntityDrag
    }
}