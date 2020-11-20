/// <summary>
/// The structure of the info.json file
/// </summary>
namespace bg3_modders_multitool.Models
{
    using System.Collections.Generic;
    class InfoJson
    {
        public List<MetaLsx> Mods { get; set; }
        public string MD5 { get; set; }
    }
}
