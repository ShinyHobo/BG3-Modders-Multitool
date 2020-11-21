/// <summary>
/// The status data model.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System.Collections.Generic;

    public class StatusData
    {
        public string StatusType { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameRef { get; set; }
        public string Description { get; set; }
        public string DescriptionRef { get; set; }
        public string DescriptionParams { get; set; }
        public string Icon { get; set; }
        //modifier FormatColor FormatStringColor
        public string ApplyEffect { get; set; }
        public string StatusEffect { get; set; }
        public string StatusEffectOverrideForItems { get; set; }
        public string StatusEffectOverride { get; set; }
        public string StatusEffectOnTurn { get; set; }
        //modifier MaterialType MaterialType
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
        //modifier StillAnimationType StatusAnimationType
        public string AnimationLoop { get; set; }
        //modifier StillAnimationPriority StillAnimPriority
        public string AnimationEnd { get; set; }
        //modifier SoundVocalStart SoundVocalType
        //modifier SoundVocalLoop SoundVocalType
        public string SoundStart { get; set; }
        public string SoundLoop { get; set; }
        public string SoundStop { get; set; }
        //modifier SoundVocalEnd SoundVocalType
        public List<AttributeFlag> ImmuneFlag { get; set; }
        //modifier ImmuneFlag AttributeFlags
        //modifier OnApplyConditions Conditions
        public string StatsId { get; set; }
        public string StackId { get; set; }
        public int StackPriority { get; set; }
        public int AuraRadius { get; set; }
        public string AuraStatuses { get; set; } // StatsFunctors
        public string BeamEffect { get; set; }
        public string AuraFX { get; set; }
        //modifier HealStat StatusHealType
        public string PolymorphResult { get; set; }
        public bool Instant { get; set; }
        public int HealMultiplier { get; set; }
        public string SurfaceChange { get; set; }
        //modifier HealType HealValueType
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
        //modifier BonusFromSkill Skill
        public int Charges { get; set; }
        public string LeaveAction { get; set; }
        public string HealEffectId { get; set; }
        public bool DefendTargetPosition { get; set; }
        public string DieAction { get; set; }
        //modifier VampirismType VampirismType
        public string TargetConditions { get; set; }
        public bool ForceStackOverwrite { get; set; }
        public bool Necromantic { get; set; }
        public bool Toggle { get; set; }
        //modifier TickType TickType
        public string TemplateID { get; set; }
        public bool UseLyingPickingState { get; set; }
        public string Boosts { get; set; }
        public string Rules { get; set; }
        public string StableRoll { get; set; }
        public string Passives { get; set; }
        public int StableRollDC { get; set; }
        public string RemoveConditions { get; set; } // Conditions
        public int NumStableSuccess { get; set; }
        //modifier RemoveEvents StatusEvent
        public int NumStableFailed { get; set; }
        public string TickFunctors { get; set; } // StatsFunctors
        public string OnSuccess { get; set; } // StatsFunctors
        //modifier StatusPropertyFlags StatusPropertyFlags
        public string OnRollsFailed { get; set; } // StatsFunctors
        public string OnApplyFunctors { get; set; } // StatsFunctors
        public string OnRemoveFunctors { get; set; } // StatsFunctors
        //modifier LEDEffect LEDEffectType
        //modifier StatusGroups StatusGroupFlags
    }
}
