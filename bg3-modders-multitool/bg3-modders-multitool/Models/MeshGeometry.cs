/// <summary>
/// The MeshGeometry model. Keeps track of the list of meshes for a given gameobject lod and the associated file.
/// </summary>
namespace bg3_modders_multitool.Models
{
    using System.Collections.Generic;

    public class MeshGeometry
    {
        public MeshGeometry(string file, Dictionary<string, List<MeshGeometry3DObject>> meshList)
        {
            File = file;
            var filepath = file.Split('\\');
            FileName = filepath[filepath.Length-1];
            MeshList = meshList;
        }

        public string File { get; set; }

        public string FileName { get; set; }

        public Dictionary<string, List<MeshGeometry3DObject>> MeshList { get; set; }
    }
}
