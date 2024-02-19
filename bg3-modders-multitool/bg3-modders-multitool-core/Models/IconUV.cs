/// <summary>
/// The icon uv model.
/// Contains information neccessary for looking up textures from an atlus.
/// </summary>
namespace bg3_modders_multitool.Models
{
    public class IconUV
    {
        public string MapKey { get; set; }
        public float U1 { get; set; }
        public float U2 { get; set; }
        public float V1 { get; set; }
        public float V2 { get; set; }
    }
}
