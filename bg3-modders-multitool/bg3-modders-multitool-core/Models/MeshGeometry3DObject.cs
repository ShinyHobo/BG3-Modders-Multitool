/// <summary>
/// The MeshGeometry3DObject model. Keeps track of the list of LOD relationship for a given gameobject mesh geometry.
/// </summary>
namespace bg3_modders_multitool.Models
{
    using HelixToolkit.Wpf.SharpDX;

    public class MeshGeometry3DObject
    {
        public string ObjectId { get; set; }

        public string MaterialId { get; set; }

        public string BaseMaterialId { get; set; }

        public string BaseMap { get; set; }

        public string NormalMaterialId { get; set; }

        public string NormalMap { get; set; }

        public MeshGeometry3D MeshGeometry3D { get; set; }

        public string MRAOMaterialId { get; set; }

        public string MRAOMap { get; set; }

        public string HMVYMaterialId { get; set; }

        public string HMVYMap { get; set; }

        public string CLEAMaterialId { get; set; }

        public string CLEAMap { get; set; }

        public string SlotType { get; set; }
    }
}
