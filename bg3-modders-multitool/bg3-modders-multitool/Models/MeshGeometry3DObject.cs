/// <summary>
/// The MeshGeometry3DObject model. Keeps track of the list of LOD relationship for a given gameobject mesh geometry.
/// </summary>
namespace bg3_modders_multitool.Models
{
    using HelixToolkit.Wpf.SharpDX;

    public class MeshGeometry3DObject
    {
        public MeshGeometry3DObject(string objectId, string materialId, string baseMaterialId, string baseTexture, MeshGeometry3D meshGeometry3D)
        {
            ObjectId = objectId;
            MaterialId = materialId;
            BaseMaterialId = baseMaterialId;
            BaseTexture = baseTexture;
            MeshGeometry3D = meshGeometry3D;
        }

        public string ObjectId { get; set; }

        public string MaterialId { get; set; }

        public string BaseMaterialId { get; set; }

        public string BaseTexture { get; set; }

        public MeshGeometry3D MeshGeometry3D { get; set; }
    }
}
