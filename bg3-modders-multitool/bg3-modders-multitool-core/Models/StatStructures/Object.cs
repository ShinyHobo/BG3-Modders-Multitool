/// <summary>
/// The object.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

    [CategoryOrder("Misc", 1)]
    [CategoryOrder("Damage Resistance", 2)]
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
        public float Weight { get; set; }
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
        public string UseConditions { get; set; } // Conditions
        public string FallingHitEffect { get; set; }
        public string FallingLandEffect { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> BludgeoningResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> SlashingResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> PiercingResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> AcidResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> ColdResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> FireResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> ForceResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> LightningResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> NecroticResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> PoisonResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> PsychicResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> RadiantResistance { get; set; }
        [Category("Damage Resistance")]
        public List<ResistanceFlag> ThunderResistance { get; set; }
        public int Sight { get; set; }
        public int FOV { get; set; }
        public int DarkvisionRange { get; set; }
        public int MinimumDetectionRange { get; set; }
        public ObjectSize GameSize { get; set; }
        public int SupplyValue { get; set; }
        public ObjectSize SoundSize { get; set; }
        public string PassivesOnEquip { get; set; }
        public string StatusInInventory { get; set; }

        public override StatStructure Clone()
        {
            return (Object)MemberwiseClone();
        }
    }
}
