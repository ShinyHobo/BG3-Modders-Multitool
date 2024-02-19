/// <summary>
/// The race model.
/// </summary>
namespace bg3_modders_multitool.Models.Races
{
    using System;
    using System.Collections.Generic;

    public class Race
    {
        public string Name { get; set; }
        public string DisplayNameHandle { get; set; }
        public string DisplayName { get; set; }
        public string DescriptionHandle { get; set; }
        public string Description { get; set; }
        public string ParentGuid { get; set; }
        public Guid UUID { get; set; }
        public string ProgressionTableUUID { get; set; }
        public List<Component> Components { get; set; }
    }
}
