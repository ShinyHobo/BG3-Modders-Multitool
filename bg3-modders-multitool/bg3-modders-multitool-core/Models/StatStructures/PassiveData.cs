/// <summary>
/// Passive data
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;

    public class PassiveData : StatStructure
    {
        public string DisplayName { get; set; }
        public string DisplayNameRef { get; set; }
        public string Description { get; set; }
        public string DescriptionRef { get; set; }
        public string DescriptionParams { get; set; }
        public string ExtraDescription { get; set; }
        public string ExtraDescriptionRef { get; set; }
        public string ExtraDescriptionParams { get; set; }
        public string Icon { get; set; }
        public List<PassiveFlag> Properties { get; set; }
        public string Boosts { get; set; }
        public List<StatsFunctorContext> StatsFunctorContext { get; set; }
        public List<StatsFunctorContext> BoostContext { get; set; }
        public string ToggleOnEffect { get; set; }
        public string ToggleOffEffect { get; set; }

        // Conditions
        public string Conditions { get; set; }
        public string BoostConditions { get; set; }

        // StatsFunctors
        public string StatsFunctors { get; set; }
        public string ToggleOnFunctors { get; set; }
        public string ToggleOffFunctors { get; set; }
        public string TooltipUseCosts { get; set; } // TODO - Add ActionPoint type enum list
        public string EnabledConditions { get; set; }
        public List<StatsFunctorContext> EnabledContext { get; set; }
        public StatsFunctorContext ToggleOffContext { get; set; }
        public string ToggleGroup { get; set; } // TODO - Determine list of togglegroups
        public string TooltipSave { get; set; } // TODO - Determine what these are and add enum list
        public int PriorityOrder { get; set; }
        public string TooltipConditionalDamage { get; set; }
        public string TooltipPermanentWarnings { get; set; }
        public Guid DynamicAnimationTag { get; set; }

        public override StatStructure Clone()
        {
            return (PassiveData)MemberwiseClone();
        }
    }
}
