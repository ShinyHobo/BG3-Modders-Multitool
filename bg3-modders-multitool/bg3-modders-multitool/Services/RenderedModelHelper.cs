/// <summary>
/// The 3d model helper.
/// </summary>
namespace bg3_modders_multitool.Services
{

    using HelixToolkit.Wpf.SharpDX;
    using HelixToolkit.Wpf.SharpDX.Assimp;
    using HelixToolkit.Wpf.SharpDX.Model.Scene;
    using System.IO;

    public static class RenderedModelHelper
    {
        /// <summary>
        /// Gets the model geometry for a given GameObject.
        /// </summary>
        /// <param name="gameObjectAttributes">The GameObject attributes.</param>
        /// <returns>The model mesh geometry.</returns>
        public static MeshGeometry3D GetMesh(System.Collections.Generic.List<Models.GameObjects.GameObjectAttribute> gameObjectAttributes)
        {
            // Get model for loaded object (.GR2)
            var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Public\Shared\Assets\Characters\_Models\_Creatures\Dragon_Red\Dragon_Red_A";
            if (!File.Exists($"{filename}.dae"))
            {
                // convert .GR2 file to .dae with divine.exe (get rid of skeleton?)
            }
            var importer = new Importer();
            // Update material here?
            var file = importer.Load($"{filename}.dae");
            // Get correct item meshnode (multiple might be due to skeleton export)
            var meshNode = file.Root.Items[0].Items[1] as MeshNode;
            var meshGeometry = meshNode.Geometry as MeshGeometry3D;
            meshGeometry.Normals = meshGeometry.CalculateNormals();
            
            importer.Dispose();
            return meshGeometry;
        }
    }
}
