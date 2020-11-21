/// <summary>
/// The object.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;

    class Object : StatStructure
    {
        public string RootTemplate { get; set; }
        public int Level { get; set; }
        public int ValueLevel { get; set; }
        public Guid ValueUUID { get; set; }
        public float ValueScale { get; set; }
        public int ValueRounding { get; set; }
        public int ValueOverride { get; set; }
        public Rarity Rarity { get; set; }
        public int Weight { get; set; }
        public string ComboCategory { get; set; }
        public string Requirements { get; set; } // Requirements
        public int Vitality { get; set; }
        public int Armor { get; set; }
        public List<AttributeFlag> Flags { get; set; }
        public string DefaultBoosts { get; set; }
        public string PersonalStatusImmunities { get; set; } // StatusIDs
        public InventoryTabs InventoryTab { get; set; }
        public bool AddToBottomBar { get; set; }
        public bool IgnoredByAI { get; set; }
        public string ObjectCategory { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public int Priority { get; set; }
        public int Unique { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public string UseCosts { get; set; }
        public ItemUseTypes ItemUseType { get; set; }
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
        public string UseConditions { get; set; } // Conditions
        public string FallingHitEffect { get; set; }
        public string FallingLandEffect { get; set; }
    }
}
