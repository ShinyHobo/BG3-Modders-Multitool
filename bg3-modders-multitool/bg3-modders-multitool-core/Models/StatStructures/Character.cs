/// <summary>
/// The character class.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

    [CategoryOrder("Misc", 1)]
    [CategoryOrder("Ability Scores",2)]
    [CategoryOrder("Armor", 3)]
    [CategoryOrder("Damage Resistance", 4)]
    public class Character : StatStructure
    {
        public int Level { get; set; }
        public int Vitality { get; set; }
        public Guid XPReward { get; set; }
        public int Sight { get; set; }
        public int Hearing { get; set; }
        public int FOV { get; set; }
        public float Weight { get; set; }
        public StepsType StepsType { get; set; }
        public string ExtraProperties { get; set; }
        public List<AttributeFlag> Flags { get; set; }
        public string DefaultBoosts { get; set; }
        public string PersonalStatusImmunities { get; set; } // StatusIDs
        public string PathInfluence { get; set; }
        public ProgressionType ProgressionType { get; set; }
        public int ProficiencyBonus { get; set; }
        public Ability SpellCastingAbility { get; set; }
        public Ability UnarmedAttackAbility { get; set; }
        public Ability UnarmedRangedAttackAbility { get; set; }
        public string ActionResources { get; set; }
        public string Class { get; set; }
        public string Passives { get; set; }
        public int Initiative { get; set; }
        public string Progressions { get; set; }
        public string MinimumDetectionRange { get; set; }
        public string DarkvisionRange { get; set; }
        public string FallingHitEffect { get; set; }
        public string FallingLandEffect { get; set; }
        public ObjectSize GameSize { get; set; }
        public List<ProficiencyGroupFlags> ProficiencyGroup { get; set; }
        public ObjectSize SoundSize { get; set; }
        public Guid ProficiencyBonusScaling { get; set; }
        public string DifficultyStatuses { get; set; }
        public Guid DynamicAnimationTag { get; set; }

        #region Armor
        [Category("Armor")]
        public int Armor { get; set; }
        [Category("Armor")]
        public ArmorType ArmorType { get; set; }
        #endregion

        #region Ability Scores
        [Category("Ability Scores")]
        public int Strength { get; set; }
        [Category("Ability Scores")]
        public int Dexterity { get; set; }
        [Category("Ability Scores")]
        public int Constitution { get; set; }
        [Category("Ability Scores")]
        public int Intelligence { get; set; }
        [Category("Ability Scores")]
        public int Wisdom { get; set; }
        [Category("Ability Scores")]
        public int Charisma { get; set; }
        #endregion

        #region Damage Resistance
        [Category("Damage Resistance")]
        public ResistanceFlag BludgeoningResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag SlashingResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag PiercingResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag AcidResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag ColdResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag FireResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag ForceResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag LightningResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag NecroticResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> PoisonResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag PsychicResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag RadiantResistance { get; set; }
        [Category("Damage Resistance")]
        public ResistanceFlag ThunderResistance { get; set; }
        #endregion

        public override StatStructure Clone()
        {
            return (Character)MemberwiseClone();
        }
    }
}