/// <summary>
/// The status data model.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;

    public class StatusData : StatStructure
    {
        public string StatusType { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameRef { get; set; }
        public string Description { get; set; }
        public string DescriptionRef { get; set; }
        public string DescriptionParams { get; set; }
        public string Icon { get; set; }
        public FormatStringColor FormatColor { get; set; }
        public string ApplyEffect { get; set; }
        public string StatusEffect { get; set; }
        public string StatusEffectOverrideForItems { get; set; }
        public string StatusEffectOverride { get; set; }
        public string StatusEffectOnTurn { get; set; }
        public MaterialType MaterialType { get; set; }
        public string Material { get; set; }
        public bool MaterialApplyBody { get; set; }
        public bool PlayerSameParty { get; set; }
        public bool MaterialApplyArmor { get; set; }
        public string PlayerHasTag { get; set; }
        public bool MaterialApplyWeapon { get; set; }
        public bool MaterialApplyNormalMap { get; set; }
        public bool PeaceOnly { get; set; }
        public int MaterialFadeAmount { get; set; }
        public int MaterialOverlayOffset { get; set; }
        public string MaterialParameters { get; set; }
        public string AnimationStart { get; set; }
        public StatusAnimationType StillAnimationType { get; set; }
        public string AnimationLoop { get; set; }
        public StillAnimPriority StillAnimationPriority { get; set; }
        public string AnimationEnd { get; set; }
        public SoundVocalType SoundVocalStart { get; set; }
        public SoundVocalType SoundVocalLoop { get; set; }
        public string SoundStart { get; set; }
        public string SoundLoop { get; set; }
        public string SoundStop { get; set; }
        public SoundVocalType SoundVocalEnd { get; set; }
        public List<AttributeFlag> ImmuneFlag { get; set; }
        public string OnApplyConditions { get; set; } // Conditions
        public string StatsId { get; set; }
        public string StackId { get; set; }
        public int StackPriority { get; set; }
        public int AuraRadius { get; set; }
        public string AuraStatuses { get; set; } // StatsFunctors
        public string BeamEffect { get; set; }
        public string AuraFX { get; set; }
        public StatusHealType HealStat { get; set; }
        public string PolymorphResult { get; set; }
        public bool Instant { get; set; }
        public int HealMultiplier { get; set; }
        public string SurfaceChange { get; set; }
        public HealValueType HealType { get; set; }
        public bool DisableInteractions { get; set; }
        public string Spells { get; set; }
        public string HealValue { get; set; }
        public string AiCalculationSpellOverride { get; set; }
        public string TargetEffect { get; set; }
        public string Items { get; set; }
        public string AbsorbSurfaceType { get; set; }
        public int FreezeTime { get; set; }
        public string Projectile { get; set; }
        public string WeaponOverride { get; set; }
        public int AbsorbSurfaceRange { get; set; }
        public string RetainSpells { get; set; }
        public int Radius { get; set; }
        public string ResetCooldowns { get; set; }
        public Skill BonusFromSkill { get; set; }
        public int Charges { get; set; }
        public string LeaveAction { get; set; }
        public string HealEffectId { get; set; }
        public bool DefendTargetPosition { get; set; }
        public string DieAction { get; set; }
        public VampirismType VampirismType { get; set; }
        public string TargetConditions { get; set; }
        public bool ForceStackOverwrite { get; set; }
        public bool Necromantic { get; set; }
        public bool Toggle { get; set; }
        public TickType TickType { get; set; }
        public string TemplateID { get; set; }
        public bool UseLyingPickingState { get; set; }
        public string Boosts { get; set; }
        public string Rules { get; set; }
        public string StableRoll { get; set; }
        public string Passives { get; set; }
        public int StableRollDC { get; set; }
        public string RemoveConditions { get; set; } // Conditions
        public int NumStableSuccess { get; set; }
        public List<StatusEvent> RemoveEvents { get; set; }
        public int NumStableFailed { get; set; }
        public string TickFunctors { get; set; } // StatsFunctors
        public string OnSuccess { get; set; } // StatsFunctors
        public List<StatusPropertyFlags> StatusPropertyFlags { get; set; }
        public string OnRollsFailed { get; set; } // StatsFunctors
        public string OnApplyFunctors { get; set; } // StatsFunctors
        public string OnRemoveFunctors { get; set; } // StatsFunctors
        public LEDEffectType LEDEffect { get; set; }
        public List<StatusGroupFlags> StatusGroups { get; set; }
        public SpellHitAnimationType HitAnimationType { get; set; }
        public float SplatterDirtAmount { get; set; }
        public float SplatterBloodAmount { get; set; }
        public float SplatterSweatAmount { get; set; }
        public string TooltipDamage { get; set; }
        public Ability TooltipSave { get; set; }
        public string OnTickRoll { get; set; }
        public string OnTickSuccess { get; set; }
        public string OnTickFail { get; set; }
        public bool IsUnique { get; set; }
        public string OnApplyRoll { get; set; }
        public string OnApplySuccess { get; set; }
        public string OnApplyFail { get; set; }
        public string StatusSoundState { get; set; }
        public List<string> AnimationSetOverride { get; set; }
        public List<string> MeshEffect { get; set; }
        public string UseCosts { get; set; } // TODO - Create model for this
        public StatusStackType StackType { get; set; }
        public string ManagedStatusEffectType { get; set; }
        public string ManagedStatusEffectGroup { get; set; }
        public string PerformEventName { get; set; }
        public Guid DynamicAnimationTag { get; set; }
        public StatusSheathing Sheathing { get; set; }
        public string TooltipPermanentWarnings { get; set; }
        public List<AuraFlag> AuraFlags { get; set; }
        public bool HideOverheadUI {get; set; }
        public Guid EndEffect { get; set; }

        public override StatStructure Clone()
        {
            return (StatusData)MemberwiseClone();
        }
    }
}
