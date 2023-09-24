/// <summary>
/// The interupt model.
/// </summary>
namespace bg3_modders_multitool.Models.StatStructures
{
    using bg3_modders_multitool.Enums.ValueLists;
    using System;
    using System.Collections.Generic;

    public class Interrupt : StatStructure
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string DescriptionParams { get; set; }
        public string ExtraDescription { get; set; }
        public string ExtraDescriptionParams { get; set; }
        public string Icon { get; set; }
        public List<InterruptContext> InterruptContext { get; set; }
        public InterruptContextScope InterruptContextScope { get; set; }
        public string Container { get; set; }
        public string Conditions { get; set; }
        public List<string> Properties { get; set; }
        public List<string> Cost { get; set; }
        public List<InterruptDefaultValue> InterruptDefaultValue { get; set; }
        public string Stack { get; set; }
        public string Roll { get; set; }
        public string Success { get; set; }
        public string Failure { get; set; }
        public string EnableCondition { get; set; }
        public string EnableContext { get; set; }
        public CooldownType Cooldown { get; set; }

        public override StatStructure Clone()
        {
            return (Interrupt)MemberwiseClone();
        }
    }
}
