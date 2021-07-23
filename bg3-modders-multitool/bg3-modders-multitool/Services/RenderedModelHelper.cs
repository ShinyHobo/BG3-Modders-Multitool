/// <summary>
/// The 3d model helper.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Models;
    using HelixToolkit.Wpf.SharpDX;
    using HelixToolkit.Wpf.SharpDX.Assimp;
    using HelixToolkit.Wpf.SharpDX.Model.Scene;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;

    public static class RenderedModelHelper
    {
        /// <summary>
        /// Looks up and loads all the model geometry for the gameobject.
        /// </summary>
        /// <param name="type">The gameobject type.</param>
        /// <param name="template">The template id.</param>
        /// <param name="bodySetVisuals">The body set visualbanks file lookup.</param>
        /// <param name="characterVisualBanks">The character visualbanks file lookup.</param>
        /// <param name="visualBanks">The visualbanks file lookup.</param>
        /// <returns>The list of geometry lookups.</returns>
        public static List<MeshGeometry> GetMeshes(string type, string template, Dictionary<string, string> characterVisualBanks, Dictionary<string, string> visualBanks, Dictionary<string, string> bodySetVisuals)
        {
            //var importFormats = Importer.SupportedFormats;
            //var exportFormats = HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormats;

            var gr2Files = new List<string>();
            var materials = new Dictionary<string, string>();

            // Lookup CharacterVisualBank file from CharacterVisualResourceID
            // CharacterVisualResourceID => characters, load CharacterVisualBank, then VisualBanks
            // VisualTemplate => items, load VisualBanks
            switch (type)
            {
                case "character":
                    gr2Files.AddRange(LoadCharacterVisualResources(template, characterVisualBanks, visualBanks, bodySetVisuals));
                    break;
                case "item":
                case "scenery":
                case "TileConstruction":
                    var itemVisualResource = LoadVisualResource(template, visualBanks);
                    if (itemVisualResource != null)
                    {
                        materials = LoadMaterials(template, visualBanks);
                        gr2Files.Add(itemVisualResource);
                    }
                    break;
                default:
                    break;
            }

            var geometryGroup = new List<MeshGeometry>();

            foreach(var gr2File in gr2Files)
            {
                var geometry = GetMesh(gr2File, materials);
                if(geometry != null)
                {
                    geometryGroup.Add(new MeshGeometry(gr2File.Replace($"{Directory.GetCurrentDirectory()}\\UnpackedData\\", string.Empty).Replace('/', '\\'), geometry));
                }
            }

            return geometryGroup;
        }

        /// <summary>
        /// Gets the model geometry from a given .GR2 file and organizes it by LOD level.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="materials">The materials list.</param>
        /// <returns>The geometry lookup table.</returns>
        public static Dictionary<string, List<MeshGeometry3DObject>> GetMesh(string filename, Dictionary<string, string> materials)
        {
            var file = LoadFile(filename);
            if (file == null)
                return null;

            // Gather meshes
            var meshes = file.Root.Items.Where(x => x.Items.Any(y => y as MeshNode != null)).ToList();
            // Group meshes by lod
            var meshGroups = meshes.GroupBy(mesh => mesh.Name.Split('_').Last()).ToList();
            var geometryLookup = new Dictionary<string, List<MeshGeometry3DObject>>();
            foreach (var meshGroup in meshGroups)
            {
                var geometryList = new List<MeshGeometry3DObject>();

                // Selecting body first
                foreach(var mesh in meshGroup)
                {
                    var name = mesh.Name.Split('-').First();
                    materials.TryGetValue(name, out string material);
                    var meshNode = mesh.Items.Last() as MeshNode;
                    var meshGeometry = meshNode.Geometry as MeshGeometry3D;
                    meshGeometry.Normals = meshGeometry.CalculateNormals();
                    geometryList.Add(new MeshGeometry3DObject(name, material, meshGeometry));
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
            try
            {
                var importer = new Importer();
                // Update material here?
                var file = importer.Load(dae);
                if (file == null && File.Exists(dae))
                {
                    GeneralHelper.WriteToConsole("Fixing vertices...\n");
                    try
                    {
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
                    catch (Exception ex)
                    {
                        // in use by another process
                        GeneralHelper.WriteToConsole($"Error : {ex.Message}\n");
                    }
                }
                
                if(!File.Exists($"{filename}.fbx"))
                {
                    var converter = new Assimp.AssimpContext();
                    var exportFormats = converter.GetSupportedExportFormats().Select(e => e.FormatId);
                    var importFormats = converter.GetSupportedImportFormats();
                    var imported = converter.ImportFile(dae);
                    converter.ExportFile(imported, $"{filename}.fbx", "fbx");
                }
                importer.Dispose();
                return file;
            }
            catch(Exception ex)
            {
                GeneralHelper.WriteToConsole($"Error loading .dae: {ex.Message}. Inner exception: {ex.InnerException?.Message}\n");
            }
            return null;
        }

        /// <summary>
        /// Finds the charactervisualresources for a given character.
        /// </summary>
        /// <param name="id">The character visualresource id.</param>
        /// <param name="characterVisualBanks">The character visualbanks lookup.</param>
        /// <param name="visualBanks">The visualbanks lookup.</param>
        /// <returns>The list of character visual resources found.</returns>
        private static List<string> LoadCharacterVisualResources(string id, Dictionary<string, string> characterVisualBanks, Dictionary<string, string> visualBanks, Dictionary<string, string> bodySetVisuals)
        {
            var characterVisualResources = new List<string>();
            if (id != null)
            {
                characterVisualBanks.TryGetValue(id, out string file);
                if (file != null)
                {
                    var xml = XDocument.Load(FileHelper.GetPath(file));
                    var characterVisualResource = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" && x.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == id).First();
                    var bodySetVisualId = characterVisualResource.Elements("attribute").Single(x => x.Attribute("id").Value == "BodySetVisual").Attribute("value").Value;
                    var bodySetVisual = LoadVisualResource(bodySetVisualId, visualBanks);
                    if(bodySetVisual != null)
                        characterVisualResources.Add(bodySetVisual);
                    var slots = characterVisualResource.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Slots").ToList();
                    foreach (var slot in slots)
                    {
                        var visualResourceId = slot.Elements("attribute").SingleOrDefault(a => a.Attribute("id").Value == "VisualResource").Attribute("value").Value;
                        var visualResource = LoadVisualResource(visualResourceId, visualBanks);
                        if (visualResource != null)
                            characterVisualResources.Add(visualResource);
                    }
                }
            }
            return characterVisualResources;
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

        /// <summary>
        /// Finds the materials used for each LOD level of an object's components.
        /// </summary>
        /// <param name="id">The id to match.</param>
        /// <param name="visualBanks">The visualbanks to lookup.</param>
        /// <returns>The list of material/lod relationships.</returns>
        private static Dictionary<string, string> LoadMaterials(string id, Dictionary<string, string> visualBanks)
        {
            if (id !=null)
            {
                visualBanks.TryGetValue(id, out string visualResourceFile);
                if (visualResourceFile != null)
                {
                    var xml = XDocument.Load(FileHelper.GetPath(visualResourceFile));
                    var visualResourceNode = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" && x.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == id).First();
                    var children = visualResourceNode.Element("children");
                    if (children != null)
                    {
                        var materialIds = new Dictionary<string, string>();
                        var nodes = children.Elements("node");
                        foreach (XElement node in nodes.Where(node => node.Attribute("id").Value == "Objects"))
                        {
                                var materialId = node.Elements("attribute").Single(a => a.Attribute("id").Value == "MaterialID").Attribute("value").Value;
                                var objectId = node.Elements("attribute").Single(a => a.Attribute("id").Value == "ObjectID").Attribute("value").Value;
                                if (materialId != null)
                                    materialIds.Add(objectId.Split('.')[1], materialId);
                        }
                        return materialIds;
                    }
                }
            }
            return null;
        }
    }
}
