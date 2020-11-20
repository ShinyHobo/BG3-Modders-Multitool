/// <summary>
/// The race model.
/// </summary>
namespace bg3_modders_multitool.Models.Races
{
    using System.Collections.Generic;

    public class Race
    {
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string ParentGuid { get; set; }
        public string UUID { get; set; }
        public string ProgressionTableUUID { get; set; }
        public List<Component> Components { get; set; }
    }
}
