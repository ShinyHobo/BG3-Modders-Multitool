/// <summary>
/// Passive data
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
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
        public StatsFunctorContext StatsFunctorContext { get; set; }
        public StatsFunctorContext BoostContext { get; set; }
        public string ToggleOnEffect { get; set; }
        public string ToggleOffEffect { get; set; }

        // Conditions
        public string Conditions { get; set; }
        public string BoostConditions { get; set; }

        // StatsFunctors
        public string StatsFunctors { get; set; }
        public string ToggleOnFunctors { get; set; }
        public string ToggleOffFunctors { get; set; }
    }
}
