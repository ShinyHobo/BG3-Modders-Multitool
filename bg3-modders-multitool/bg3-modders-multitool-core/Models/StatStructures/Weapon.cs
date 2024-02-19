/// <summary>
/// The weapon model.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;

    public class Weapon : StatStructure
    {
        public string RootTemplate { get; set; }
        public string ItemGroup { get; set; }
        public int Level { get; set; }
        public string Requirements { get; set; } // Requirements
        public string UseConditions { get; set; } // Conditions
        public DamageType DamageType { get; set; }
        public string Damage { get; set; }
        public int DamageRange { get; set; }
        public int WeaponRange { get; set; }
        public int Durability { get; set; }
        public int? DurabilityDegradeSpeed { get; set; } // Qualifier (null, 0-10)
        public int ValueLevel { get; set; }
        public Guid ValueUUID { get; set; }
        public float ValueScale { get; set; }
        public int ValueRounding { get; set; }
        public int ValueOverride { get; set; }
        public Rarity Rarity { get; set; }
        public float Weight { get; set; }
        public WeaponType WeaponType { get; set; }
        public ItemSlot Slot { get; set; }
        public string Projectile { get; set; }
        public bool IgnoreVisionBlock { get; set; }
        public string ComboCategory { get; set; }
        public string Spells { get; set; }
        public string Tags { get; set; }
        public string ExtraProperties { get; set; }
        public string WeaponFunctors { get; set; } // StatsFunctors
        public List<AttributeFlag> Flags { get; set; }
        public string DefaultBoosts { get; set; }
        public string PersonalStatusImmunities { get; set; } // StatusIDs
        public string Boosts { get; set; }
        public string Passives { get; set; }
        public InventoryTabs InventoryTab { get; set; }
        public bool NeedsIdentification { get; set; }
        public int Charges { get; set; }
        public int MaxCharges { get; set; }
        public string ItemColor { get; set; }
        public string ObjectCategory { get; set; }
        public int MinAmount { get; set; }
        public int MaxAmount { get; set; }
        public int Priority { get; set; }
        public int Unique { get; set; }
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public WeaponGroup WeaponGroup { get; set; }
        public string VersatileDamage { get; set; }
        public List<WeaponFlags> WeaponProperties { get; set; }
        public string FallingHitEffect { get; set; }
        public string FallingLandEffect { get; set; }
        public string ColorPresetResource { get; set; }
        public List<ProficiencyGroupFlags> ProficiencyGroup { get; set; }
        public List<string> BoostsOnEquipMainHand { get; set; }
        public string UseCosts { get; set; } // TODO - Create model for this
        public string PassivesMainHand { get; set; } // TODO - Create enum for this
        public int SupplyValue { get; set; }
        public ObjectSize GameSize { get; set; }
        public string PassivesOnEquip { get; set; }
        public string StatusOnEquip { get; set; }
        public string UniqueWeaponSoundSwitch { get; set; }
        public string PassivesOffHand { get; set; }
        public string BoostsOnEquipOffHand { get; set; }

        public override StatStructure Clone()
        {
            return (Weapon)MemberwiseClone();
        }
    }
}
