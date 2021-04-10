/// <summary>
/// The 3d model helper.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using HelixToolkit.Wpf.SharpDX;
    using HelixToolkit.Wpf.SharpDX.Assimp;
    using HelixToolkit.Wpf.SharpDX.Model.Scene;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public static class RenderedModelHelper
    {
        /// <summary>
        /// Gets the model geometry for a given GameObject.
        /// </summary>
        /// <param name="gameObjectAttributes">The GameObject attributes.</param>
        /// <returns>The model mesh geometry.</returns>
        public static MeshGeometry3D GetMesh(System.Collections.Generic.List<Models.GameObjects.GameObjectAttribute> gameObjectAttributes)
        {
            //var importFormats = Importer.SupportedFormats;
            //var exportFormats = HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormats;

            // Get model for loaded object (.GR2)
            //var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Public\Shared\Assets\Characters\_Models\_Creatures\Dragon_Red\Dragon_Red_A";
            var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Generated\Public\Shared\Assets\Characters\_Models\_Creatures\Automaton\Resources\AUTOMN_M_Body_A";
            //var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Generated\Public\Shared\Assets\Characters\_Models\_Creatures\Elementals\Resources\ELEM_Mud_Body_A";
            //var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Generated\Public\Shared\Assets\Buildings\Avernus\BLD_Avernus_Devil_Citadel_ABC\Resources\BLD_Avernus_Devil_Citadel_A";
            //var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Generated\Public\Shared\Assets\Weapons\WPN_HUM_Flameblade_A\Resources\WPN_HUM_Flameblade_A";
            //var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Generated\Public\Shared\Assets\Characters\_Models\_Creatures\Hollyphant\Resources\HPHANT_Body_A";
            var dae = $"{filename}.dae";

            if (!File.Exists(dae))
            {
                GeneralHelper.WriteToConsole($"Converting model to .dae for rendering...\n");
                var divine = $" -g \"bg3\" --action \"convert-model\" --output-format \"dae\" --source \"{filename}.GR2\" --destination \"{dae}\" -l \"all\"";
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    FileName = Properties.Settings.Default.divineExe,
                    Arguments = divine,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
                GeneralHelper.WriteToConsole(process.StandardOutput.ReadToEnd());
                GeneralHelper.WriteToConsole(process.StandardError.ReadToEnd());
            }
            var importer = new Importer();
            // Update material here?
            var file = importer.Load(dae);
            if(file == null)
            {
                GeneralHelper.WriteToConsole("Fixing vertices...\n");
                var xml = XDocument.Load(dae);
                var geometryList = xml.Descendants().Where(x => x.Name.LocalName == "geometry").ToList();
                foreach(var lod in geometryList)
                {
                    var vertexId = lod.Descendants().Where(x => x.Name.LocalName == "vertices").Select(x => x.Attribute("id").Value).First();
                    var vertex = lod.Descendants().Single(x => x.Name.LocalName == "input" && x.Attribute("semantic").Value == "VERTEX");
                    vertex.Attribute("source").Value = $"#{vertexId}";
                }
                xml.Save(dae);
                GeneralHelper.WriteToConsole("Model conversion complete!\n");
                file = importer.Load(dae);
            }
            // TODO - need lod slider
            var meshNode = file.Root.Items.Where(x => x.Items.Any(y => y as MeshNode != null)).Last().Items.Last() as MeshNode;
            var meshGeometry = meshNode.Geometry as MeshGeometry3D;
            meshGeometry.Normals = meshGeometry.CalculateNormals();
            importer.Dispose();
            return meshGeometry;
        }
    }
}
