/// <summary>
/// The armor.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;

    public class Armor : StatStructure
    {
        public string RootTemplate { get; set; }
        public string ItemGroup { get; set; }
        public int Level { get; set; }
        public ItemSlot Slot { get; set; }
        public string Requirements { get; set; } // Requirements
        public string UseConditions { get; set; } // Conditions
        public int ArmorClass { get; set; }
        public int Durability { get; set; }
        public int? DurabilityDegradeSpeed { get; set; } // Qualifier (null, 0-10)
        public int ValueLevel { get; set; }
        public Guid ValueUUID { get; set; }
        public float ValueScale { get; set; }
        public int ValueRounding { get; set; }
        public int ValueOverride { get; set; }
        public Rarity Rarity { get; set; }
        public float Weight { get; set; }
        public string Spells { get; set; }
        public string Tags { get; set; }
        public string ExtraProperties { get; set; }
        public List<AttributeFlag> Flags { get; set; }
        public string DefaultBoosts { get; set; }
        public string PersonalStatusImmunities { get; set; } // StatusIDs
        public string Boosts { get; set; }
        public string Passives { get; set; }
        public string ComboCategory { get; set; }
        public InventoryTabs InventoryTab { get; set; }
        public ArmorType ArmorType { get; set; }
        public string ItemColor { get; set; }
        public bool NeedsIdentification { get; set; }
        public int Charges { get; set; }
        public int MaxCharges { get; set; }
        public string ObjectCategory { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public int Priority { get; set; }
        public int Unique { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public bool Shield { get; set; }
        public Ability ArmorClassAbility { get; set; }
        public int AbilityModifierCap { get; set; }
        public string FallingHitEffect { get; set; }
        public string FallinglandEffect { get; set; }
        public string ColorPresetResource { get; set; }
        public string UseCosts { get; set; } // TODO - Add types, such as ActionPoints
        public List<ProficiencyGroupFlags> ProficiencyGroup { get; set; }
        public List<StatusBoost> StatusOnEquip { get; set; }
        public InstrumentType InstrumentType { get; set; }
        public string PassivesOnEquip { get; set; }

        public override StatStructure Clone()
        {
            return (Armor)MemberwiseClone();
        }
    }
}
