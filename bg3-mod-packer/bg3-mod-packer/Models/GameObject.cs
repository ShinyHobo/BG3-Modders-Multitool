/// <summary>
/// The game object model.
/// </summary>
namespace bg3_mod_packer.Models
{
    using System.Collections.Generic;

    public class GameObject
    {
        public string MapKey { get; set; }
        public string ParentTemplateId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Stats { get; set; }
        public List<GameObject> Children { get; set; }
    }
}
