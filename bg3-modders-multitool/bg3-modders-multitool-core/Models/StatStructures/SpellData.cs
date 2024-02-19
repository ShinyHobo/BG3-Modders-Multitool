/// <summary>
/// The spell data model.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;

    public class SpellData : StatStructure
    {
        public string SpellType { get; set; }
        public int Level { get; set; }
        public SpellSchool SpellSchool { get; set; }
        public string SpellContainerID { get; set; }
        public CooldownType Cooldown { get; set; }
        public string ContainerSpells { get; set; }
        public SpellRequirement Requirement { get; set; }
        public List<AIFlag> AIFlags { get; set; }
        public string SpellProperties { get; set; } // StatsFunctors
        public string TargetRadius { get; set; }
        public int Damage { get; set; }
        public int AreaRadius { get; set; }
        public int DamageRange { get; set; }
        public string AddRangeFromAbility { get; set; }
        public SurfaceType SurfaceType { get; set; }
        public DamageType DamageType { get; set; }
        public DeathType DeathType { get; set; }
        public int SurfaceLifetime { get; set; }
        public int ExplodeRadius { get; set; }
        public int SurfaceGrowStep { get; set; }
        public string AmountOfTargets { get; set; } // can be int or function, ie LevelMapValue(EldritchBlast)
        public int StrikeCount { get; set; }
        public int SurfaceGrowInterval { get; set; }
        public int MaxDistance { get; set; }
        public int Acceleration { get; set; }
        public bool AutoAim { get; set; }
        public int StrikeDelay { get; set; }
        public int TeleportDelay { get; set; }
        public string AuraSelf { get; set; }
        public string SpellRoll { get; set; } // RollConditions
        public string AuraAllies { get; set; }
        public string SpellSuccess { get; set; } // StatsFunctors
        public int MaxAttacks { get; set; }
        public int MovementSpeed { get; set; }
        public int GrowSpeed { get; set; }
        public string AuraNeutrals { get; set; }
        public string SpellFail { get; set; } // StatsFunctors
        public int NextAttackChance { get; set; }
        public int GrowTimeout { get; set; }
        public string AuraEnemies { get; set; }
        public string TargetConditions { get; set; } // TargetConditions
        public string AoEConditions { get; set; } // TargetConditions
        public int NextAttackChanceDivider { get; set; }
        public int RandomPoints { get; set; }
        public int Offset { get; set; }
        public string AuraItems { get; set; }
        public int ProjectileCount { get; set; }
        public bool TargetProjectiles { get; set; }
        public int EndPosRadius { get; set; }
        public bool OverrideSpellLevel { get; set; }
        public int TotalSurfaceCells { get; set; }
        public int ProjectileDelay { get; set; }
        public int JumpDelay { get; set; }
        public bool TeleportSelf { get; set; }
        public int Angle { get; set; }
        public float HitRadius { get; set; }
        public bool TeleportSurface { get; set; }
        public int TravelSpeed { get; set; }
        public string Template { get; set; }
        public string MemorizationRequirements { get; set; } // MemorizationRequirements
        public int Lifetime { get; set; }
        public string Icon { get; set; }
        public int Height { get; set; }
        public int MinHitsPerTurn { get; set; }
        public int SurfaceStatusChance { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameRef { get; set; }
        public bool SingleSource { get; set; }
        public int MaxHitsPerTurn { get; set; }
        public int PointsMaxOffset { get; set; }
        public string Description { get; set; }
        public string DescriptionRef { get; set; }
        public int HitDelay { get; set; }
        public string DescriptionParams { get; set; }
        public string ExtraDescription { get; set; }
        public string ExtraDescriptionRef { get; set; }
        public bool UseWeaponDamage { get; set; }
        public string ExtraDescriptionParams { get; set; }
        public bool UseWeaponProperties { get; set; }
        public string TooltipDamageList { get; set; }
        public int FXScale { get; set; }
        public string TooltipAttackSave { get; set; }
        public string TooltipStatusApply { get; set; }
        public string TooltipPermanentWarnings { get; set; }
        public string CastSelfAnimation { get; set; }
        public string PrepareEffect { get; set; }
        public string CastEffect { get; set; }
        public string PreviewEffect { get; set; }
        public string TargetEffect { get; set; }
        public string HitEffect { get; set; }
        public string TargetHitEffect { get; set; }
        public string TargetGroundEffect { get; set; }
        public string PositionEffect { get; set; }
        public string BeamEffect { get; set; }
        public string SpellEffect { get; set; }
        public string SelectedCharacterEffect { get; set; }
        public string SelectedObjectEffect { get; set; }
        public string SelectedPositionEffect { get; set; }
        public string DisappearEffect { get; set; }
        public string ReappearEffect { get; set; }
        public string ImpactEffect { get; set; }
        public string PrepareSound { get; set; }
        public string SourceTargetEffect { get; set; }
        public CursorMode PreviewCursor { get; set; }
        public string CastEffectTextEvent { get; set; }
        public string TargetTargetEffect { get; set; }
        public string FlyEffect { get; set; }
        public string CastTextEvent { get; set; }
        public string PrepareEffectBone { get; set; }
        public string Template1 { get; set; }
        public string AlternativeCastTextEvents { get; set; }
        public string WeaponBones { get; set; }
        public string Shape { get; set; }
        public string Template2 { get; set; }
        public string CastSound { get; set; }
        public ProjectileDistribution Distribution { get; set; }
        public string TargetSound { get; set; }
        public int FrontOffset { get; set; }
        public string Template3 { get; set; }
        public string DomeEffect { get; set; }
        public string VocalComponentSound { get; set; }
        public bool Shuffle { get; set; }
        public bool Autocast { get; set; }
        public int Range { get; set; }
        public string StormEffect { get; set; }
        public string SpawnEffect { get; set; }
        public bool ProjectileTerrainOffset { get; set; }
        public bool PreviewStrikeHits { get; set; }
        public int Base { get; set; }
        public string CleanseStatuses { get; set; }
        public string MovingObject { get; set; }
        public string MaleImpactEffects { get; set; }
        public string TargetCastEffect { get; set; }
        public int StatusClearChance { get; set; }
        public ProjectileType ProjectileType { get; set; }
        public string FemaleImpactEffects { get; set; }
        public string Spellbook { get; set; }
        public string StartTextEvent { get; set; }
        public string AiCalculationSpellOverride { get; set; }
        public string ReappearEffectTextEvent { get; set; }
        public string StopTextEvent { get; set; }
        public string RainEffect { get; set; }
        public string CycleConditions { get; set; } // TargetConditions
        public int Memory_Cost { get; set; }
        public string ProjectileSpells { get; set; }
        public AtmosphereType Atmosphere { get; set; }
        public string UseCosts { get; set; }
        public int MagicCost { get; set; }
        public int ConsequencesStartTime { get; set; }
        public string DualWieldingUseCosts { get; set; }
        public bool Stealth { get; set; }
        public int ConsequencesDuration { get; set; }
        public string ThrowableTargetConditions { get; set; } // TargetConditions
        public string HitCosts { get; set; }
        public string SpellAnimationArcaneMagic { get; set; }
        public int SurfaceRadius { get; set; }
        public string SpellAnimationDivineMagic { get; set; }
        public string SpellAnimationNoneMagic { get; set; }
        public string DualWieldingSpellAnimationArcaneMagic { get; set; }
        public string DualWieldingSpellAnimationDivineMagic { get; set; }
        public string DualWieldingSpellAnimationNoneMagic { get; set; }
        public string RequirementConditions { get; set; } // TargetConditions
        public VerbalIntent VerbalIntent { get; set; }
        public List<WeaponFlags> WeaponTypes { get; set; }
        public List<SpellFlagList> SpellFlags { get; set; }
        public int MaximumTotalTargetHP { get; set; }
        public SpellActionType SpellActionType { get; set; }
        public SpellAnimationType SpellAnimationType { get; set; }
        public SpellHitAnimationType SpellHitAnimationType { get; set; }
        public SpellAnimationIntentType SpellAnimationIntentType { get; set; }
        public SpellJumpType SpellJumpType { get; set; }
        public int MaximumTargets { get; set; }
        public string RechargeValues { get; set; }
        public string Requirements { get; set; } // Requirements
        public int ForkChance { get; set; }
        public int MaxForkCount { get; set; }
        public int ForkLevels { get; set; }
        public LineOfSightFlags LineOfSightFlags { get; set; }
        public string ForkingConditions { get; set; } // TargetConditions
        public int MemoryCost { get; set; }
        public string RootSpellID { get; set; }
        public int PowerLevel { get; set; }
        public int SourceLimbIndex { get; set; }
        public string TargetCeiling { get; set; }
        public float TargetFloor { get; set; }
        public List<string> Trajectories { get; set; }
        public List<string> SpellAnimation { get; set; } // TODO - Create type for this, has complex form "73afb4e5-8cfe-4479-95cf-16889597fee3(CMBT_Range_RHand_01_Prepare),,"
        public List<string> DualWieldingSpellAnimation { get; set; } // TODO - See SpellAnimation
        public SpellHitAnimationType HitAnimationType { get; set; }
        public SpellStyleGroup SpellStyleGroup { get; set; }
        public SpellCategoryFlag SpellCategory { get; set; }
        public int MinJumpDistance { get; set; }
        public bool StopAtFirstContact { get; set; }
        public float HitExtension { get; set; }
        public bool OnlyHit1Target { get; set; }
        public List<CinematicArenaFlag> CinematicArenaFlags { get; set; }
        public SpellSoundMagnitude SpellSoundMagnitude {get; set;}
        public List<RequirementEvent> RequirementEvents { get; set; }
        public string InstrumentComponentPrepareSound { get; set; }
        public string InstrumentComponentLoopingSound { get; set; }
        public string InstrumentComponentCastSound { get; set; }
        public string InstrumentComponentImpactSound { get; set; }
        public SpellSheathing Sheathing { get; set; }
        public Guid TooltipUpcastDescription { get; set; }
        public List<string> TooltipUpcastDescriptionParams { get; set; }
        public Guid TooltipOnMiss { get; set; }
        public string PrepareLoopSound { get; set; }
        public string ShortDescription { get; set; }
        public List<string> ShortDescriptionParams { get; set; }
        public string TooltipOnSave { get; set; }
        public string ConcentrationSpellID { get; set; }
        public string InterruptPrototype { get; set; }
        public List<string> OriginSpellProperties { get; set; }
        public string OriginTargetConditions { get; set; }
        public int CastTargetHitDelay { get; set; }
        public CombatAIOverrideSpell CombatAIOverrideSpell { get; set; }
        public List<string> RitualCosts { get; set; }
        public float SteerSpeedMultipler { get; set; }
        public string HighlightConditions { get; set; }
        public string ThrowableSpellRoll { get; set; }
        public string ThrowableSpellSuccess { get; set; }
        public int ForceTarget { get; set; }
        public string ItemWall { get; set; }
        public string ItemWallStatus { get; set; }
        public bool IgnoreTeleport { get; set; }
        public string OriginSpellRoll { get; set; }
        public string OriginSpellSuccess { get; set; }
        public string OriginSpellFail { get; set; }
        public string ThrowOrigin { get; set; }
        public string ThrowableSpellProperties { get; set; }
        public string FollowUpOriginalSpell { get; set; }

        public override StatStructure Clone()
        {
            return (SpellData)MemberwiseClone();
        }
    }
}
