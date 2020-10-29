/// <summary>
/// The structure of the needed data from meta.lsx
/// </summary>
namespace bg3_mod_packer.Models
{
    using System;
    public class MetaLsx
    {
        public string Author { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string UUID { get; set; }
        public DateTime Created { get; set; }

        // Legacy fields - TODO remove at later date
        public string modName { get; set; }
        public string folderName { get; set; }
    }
}
