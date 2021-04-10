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

            // GameObject (Volo)
            //  <attribute id="MapKey" type="FixedString" value="2af25a85-5b9a-4794-85d3-0bd4c4d262fa" />
            //  <attribute id="CharacterVisualResourceID" type="FixedString" value="f8103934-8d3d-cbd9-5cf6-1a8951b98e93" />
            // CharacterVisual Resource
            //  <attribute id="ID" type="FixedString" value="f8103934-8d3d-cbd9-5cf6-1a8951b98e93" />
            //  <attribute id="BaseVisual" type="FixedString" value="3773c64c-c5a9-9baf-1b85-6bee029ee044" /> Asset Id for human male base
            //  <attribute id="BodySetVisual" type="FixedString" value="38cee76d-1a75-4419-9293-52e47fda65e9" /> Asset Id for human male body A
            //  Slots list
            //      <attribute id="VisualResource" type="FixedString" value="44e7769e-bc14-8c16-40d0-8b576ceddcb1" />  Volo head (Shared\Public\Shared\Content\Assets\Characters\Humans\Heads\[PAK]_HUM_M_Head_Volo\_merged.lsx)
            //      VisualBank Resource
            //          <attribute id="ID" type="FixedString" value="44e7769e-bc14-8c16-40d0-8b576ceddcb1" />
            //          <attribute id="SourceFile" type="LSWString" value="Generated/Public/Shared/Assets/Characters/_Anims/Humans/_Male/Resources/HUM_M_NKD_Head_Volo.GR2" />
            //          child objects - match name to get materialid per part
            //      <attribute id="VisualResource" type="FixedString" value="c60465a9-71bd-b436-c837-7dfadf7edf1c" /> Volo bar shirt (Shared\Public\Shared\Content\Assets\Characters\Humans\[PAK]_Male_Clothing\_merged.lsx)
            //      VisualBank Resource
            //          <attribute id="ID" type="FixedString" value="c60465a9-71bd-b436-c837-7dfadf7edf1c" />
            //          <attribute id="SourceFile" type="LSWString" value="Generated/Public/Shared/Assets/Characters/_Models/Humans/Resources/HUM_M_CLT_Bard_Shirt_A.GR2" />
            //          child objects - match name to get materialid per part

            // Get model for loaded object (.GR2)
            var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Generated/Public/Shared/Assets/Characters/_Anims/Humans/_Male/Resources/HUM_M_NKD_Head_Volo";
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

            // Gather meshes
            var meshes = file.Root.Items.Where(x => x.Items.Any(y => y as MeshNode != null)).ToList();
            // Group meshes by lod
            var meshGroups = meshes.GroupBy(mesh => mesh.Name.Split('_').Last()).ToList();
            // TODO - need lod slider, selecting highest lod first
            var meshGroup = meshGroups[0].ToList();
            // Selecting body first
            var meshNode = meshGroup[0].Items.Last() as MeshNode;
            var meshGeometry = meshNode.Geometry as MeshGeometry3D;
            meshGeometry.Normals = meshGeometry.CalculateNormals();
            importer.Dispose();
            return meshGeometry;
        }
    }
}
