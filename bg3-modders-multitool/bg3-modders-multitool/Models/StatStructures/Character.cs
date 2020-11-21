/// <summary>
/// The character class.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;

    public class Character
    {
        public int Level { get; set; }
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Constitution { get; set; }
        public int Intelligence { get; set; }
        public int Wisdom { get; set; }
        public int Charisma { get; set; }
        public int Armor { get; set; }
        public int Vitality { get; set; }
        public Guid XPReward { get; set; }
        public int Sight { get; set; }
        public int Hearing { get; set; }
        public int FOV { get; set; }
        public int Weight { get; set; }
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
        public string ActionResources { get; set; }
        public string Class { get; set; }
        public string Passives { get; set; }
        public List<ResistanceFlag> BludgeoningResistance { get; set; }
        public List<ResistanceFlag> SlashingResistance { get; set; }
        public List<ResistanceFlag> PiercingResistance { get; set; }
        public List<ResistanceFlag> AcidResistance { get; set; }
        public List<ResistanceFlag> ColdResistance { get; set; }
        public List<ResistanceFlag> FireResistance { get; set; }
        public List<ResistanceFlag> ForceResistance { get; set; }
        public List<ResistanceFlag> LightningResistance { get; set; }
        public List<ResistanceFlag> NecroticResistance { get; set; }
        public List<ResistanceFlag> PoisonResistance { get; set; }
        public List<ResistanceFlag> PsychicResistance { get; set; }
        public List<ResistanceFlag> RadiantResistance { get; set; }
        public List<ResistanceFlag> ThunderResistance { get; set; }
        public int Initiative { get; set; }
        public string Progressions { get; set; }
        public string MinimumDetectionRange { get; set; }
        public string DarkvisionsRange { get; set; }
        public string FallingHitEffect { get; set; }
        public string FallingLandEffect { get; set; }
        public ArmorType ArmorType { get; set; }
        public ProficiencyGroupFlags Proficiencygroup { get; set; }
    }
}
