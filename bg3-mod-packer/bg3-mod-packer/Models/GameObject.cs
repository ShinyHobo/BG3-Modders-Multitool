/// <summary>
/// The game object model.
/// </summary>
namespace bg3_mod_packer.Models
{
    using System.Collections.Generic;
    using System.Linq;

    public class GameObject
    {
        public string MapKey { get; set; }
        public string ParentTemplateId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Stats { get; set; }
        public List<GameObject> Children { get; set; }

        public int Depth {
            get {
                if (Children.Count == 0)
                    return 0;
                return Children.Select(x => x.Depth).DefaultIfEmpty().Max() + 1;
            }
        }

        public int Count {
            get {
                if (Children == null)
                    return 0;
                return Children.Sum(x => x.Count) + Children.Count;
            }
        }
    }
}
