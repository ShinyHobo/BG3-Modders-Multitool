/// <summary>
/// The 3d model helper.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.Models.GameObjectTypes;
    using HelixToolkit.Wpf.SharpDX;
    using HelixToolkit.Wpf.SharpDX.Assimp;
    using HelixToolkit.Wpf.SharpDX.Model.Scene;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public static class RenderedModelHelper
    {
        /// <summary>
        /// Looks up and loads all the model geometry for the gameobject.
        /// </summary>
        /// <param name="gameObjectAttributes">The gameobject attributes to get the necessary lookup information from.</param>
        /// <param name="characterVisualBanks">The character visualbanks file lookup.</param>
        /// <param name="visualBanks">The visualbanks file lookup.</param>
        /// <returns>The list of geometry lookups.</returns>
        public static List<Dictionary<string, List<MeshGeometry3D>>> GetMeshes(List<Models.GameObjects.GameObjectAttribute> gameObjectAttributes, Dictionary<string, string> characterVisualBanks, Dictionary<string, string> visualBanks)
        {
            //var importFormats = Importer.SupportedFormats;
            //var exportFormats = HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormats;

            var gr2Files = new List<string>();

            // Check GameObject type
            var type = (FixedString)gameObjectAttributes.Single(goa => goa.Name == "Type").Value;

            // Lookup CharacterVisualBank file from CharacterVisualResourceID
            // CharacterVisualResourceID => characters, load CharacterVisualBank, then VisualBanks
            // VisualTemplate => items, load VisualBanks
            switch (type)
            {
                case "character":
                    var characterVisualTemplate = (FixedString)gameObjectAttributes.SingleOrDefault(goa => goa.Name == "CharacterVisualResourceID")?.Value;
                    if(characterVisualTemplate != null)
                    {
                        characterVisualBanks.TryGetValue(characterVisualTemplate, out string file);
                        if(file != null)
                        {
                            var xml = XDocument.Load(FileHelper.GetPath(file));
                            var characterVisualResource = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" && x.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == characterVisualTemplate).First();
                            var slots = characterVisualResource.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Slots").ToList();
                            foreach(var slot in slots)
                            {
                                var visualResourceId = slot.Elements("attribute").SingleOrDefault(a => a.Attribute("id").Value == "VisualResource").Attribute("value").Value;
                                var visualResource = LoadVisualResource(visualResourceId, visualBanks);
                                if (visualResource != null)
                                    gr2Files.Add(visualResource);
                            }
                        }
                    }
                    break;
                case "item":
                case "scenery":
                case "TileConstruction":
                    var visualTemplate = (FixedString)gameObjectAttributes.SingleOrDefault(goa => goa.Name == "VisualTemplate")?.Value;
                    var itemVisualResource = LoadVisualResource(visualTemplate, visualBanks);
                    if (itemVisualResource != null)
                        gr2Files.Add(itemVisualResource);
                    break;
                default:
                    break;
            }

            var geometryGroup = new List<Dictionary<string, List<MeshGeometry3D>>>();

            foreach(var gr2File in gr2Files)
            {
                var geometry = GetMesh(gr2File);
                if(geometry != null)
                    geometryGroup.Add(geometry);
            }

            return geometryGroup;
        }

        /// <summary>
        /// Gets the model geometry from a given .GR2 file and organizes it by LOD level.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The geometry lookup table.</returns>
        public static Dictionary<string, List<MeshGeometry3D>> GetMesh(string filename)
        {
            var file = LoadFile(filename);
            if (file == null)
                return null;

            // Gather meshes
            var meshes = file.Root.Items.Where(x => x.Items.Any(y => y as MeshNode != null)).ToList();
            // Group meshes by lod
            var meshGroups = meshes.GroupBy(mesh => mesh.Name.Split('_').Last()).ToList();
            var geometryLookup = new Dictionary<string, List<MeshGeometry3D>>();
            foreach (var meshGroup in meshGroups)
            {
                var geometryList = new List<MeshGeometry3D>();

                // Selecting body first
                foreach(var mesh in meshGroup)
                {
                    var meshNode = mesh.Items.Last() as MeshNode;
                    var meshGeometry = meshNode.Geometry as MeshGeometry3D;
                    meshGeometry.Normals = meshGeometry.CalculateNormals();
                    geometryList.Add(meshGeometry);
                }
                geometryLookup.Add(meshGroup.Key, geometryList);
            }
            return geometryLookup;
        }

        /// <summary>
        /// Converts a given .GR2 file to a .dae file for rendering and further conversion.
        /// </summary>
        /// <param name="filename">The file name.</param>
        /// <returns>The .dae converted model file.</returns>
        private static HelixToolkitScene LoadFile(string filename)
        {
            var dae = $"{filename}.dae";

            if (!File.Exists(dae))
            {
                GeneralHelper.WriteToConsole($"Converting model to .dae for rendering...\n");
                var divine = $" -g \"bg3\" --action \"convert-model\" --output-format \"dae\" --source \"\\\\?\\{filename}.GR2\" --destination \"\\\\?\\{dae}\" -l \"all\"";
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
            if (file == null && File.Exists(dae))
            {
                GeneralHelper.WriteToConsole("Fixing vertices...\n");
                var xml = XDocument.Load(dae);
                var geometryList = xml.Descendants().Where(x => x.Name.LocalName == "geometry").ToList();
                foreach (var lod in geometryList)
                {
                    var vertexId = lod.Descendants().Where(x => x.Name.LocalName == "vertices").Select(x => x.Attribute("id").Value).First();
                    var vertex = lod.Descendants().Single(x => x.Name.LocalName == "input" && x.Attribute("semantic").Value == "VERTEX");
                    vertex.Attribute("source").Value = $"#{vertexId}";
                }
                xml.Save(dae);
                GeneralHelper.WriteToConsole("Model conversion complete!\n");
                file = importer.Load(dae);
            }
            importer.Dispose();
            return file;
        }

        /// <summary>
        /// Finds the visualresource sourcefile for an object.
        /// </summary>
        /// <param name="id">The id to match.</param>
        /// <param name="visualBanks">The visualbanks lookup.</param>
        /// <returns>The .GR2 sourcefile.</returns>
        private static string LoadVisualResource(string id, Dictionary<string, string> visualBanks)
        {
            if (id != null)
            {
                visualBanks.TryGetValue(id, out string visualResourceFile);
                if (visualResourceFile != null)
                {
                    var xml = XDocument.Load(FileHelper.GetPath(visualResourceFile));
                    var visualResourceNode = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" && x.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == id).First();
                    var gr2File = visualResourceNode.Elements("attribute").SingleOrDefault(a => a.Attribute("id").Value == "SourceFile")?.Attribute("value").Value;
                    if (gr2File == null)
                        return null;
                    gr2File = gr2File.Replace(".GR2", string.Empty);
                    return FileHelper.GetPath($"Models\\{gr2File}");
                }
            }
            return null;
        }
    }
}
