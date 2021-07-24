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
        /// <param name="materialBanks">The materialBanks file lookup.</param>
        /// <param name="textureBanks">The texturebanks file lookup.</param>
        /// <returns>The list of geometry lookups.</returns>
        public static List<MeshGeometry> GetMeshes(string type, string template, Dictionary<string, string> characterVisualBanks, Dictionary<string, string> visualBanks,
            Dictionary<string, string> bodySetVisuals, Dictionary<string, string> materialBanks, Dictionary<string, string> textureBanks)
        {
            //var importFormats = Importer.SupportedFormats;
            //var exportFormats = HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormats;

            var gr2Files = new List<string>();
            var materials = new Dictionary<string, Tuple<string, string>>();
            var slotTypes = new Dictionary<string, string>();

            // Lookup CharacterVisualBank file from CharacterVisualResourceID
            // CharacterVisualResourceID => characters, load CharacterVisualBank, then VisualBanks
            // VisualTemplate => items, load VisualBanks
            switch (type)
            {
                case "character":
                    var characterVisualResources = LoadCharacterVisualResources(template, characterVisualBanks, visualBanks);
                    slotTypes = characterVisualResources.Item3;
                    materials = characterVisualResources.Item2;
                    gr2Files.AddRange(characterVisualResources.Item1);
                    break;
                case "item":
                case "scenery":
                case "TileConstruction":
                    materials = LoadMaterials(template, visualBanks);
                    var itemVisualResource = LoadVisualResource(template, visualBanks);
                    if (itemVisualResource != null)
                    {
                        gr2Files.Add(itemVisualResource);
                    }
                    break;
                default:
                    break;
            }

            var geometryGroup = new List<MeshGeometry>();

            foreach(var gr2File in gr2Files)
            {
                var geometry = GetMesh(gr2File, materials, slotTypes, materialBanks, textureBanks);
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
        /// <param name="materialBanks">The materialbanks lookup.</param>
        /// <param name="textureBanks">The texturebanks lookup.</param>
        /// <returns>The geometry lookup table.</returns>
        public static Dictionary<string, List<MeshGeometry3DObject>> GetMesh(string filename, Dictionary<string, Tuple<string, string>> materials,
            Dictionary<string, string> slotTypes, Dictionary<string, string> materialBanks, Dictionary<string, string> textureBanks)
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
                    Tuple<string, string> materialGuid = null;
                    materials?.TryGetValue(name, out materialGuid);
                    var meshNode = mesh.Items.Last() as MeshNode;
                    var meshGeometry = meshNode.Geometry as MeshGeometry3D;
                    var baseMaterialId = LoadMaterial(materialGuid.Item1, "basecolor", materialBanks);
                    if (baseMaterialId == null)
                        baseMaterialId = LoadMaterial(materialGuid.Item1, "Body_color_texture", materialBanks);
                    var baseTexture = LoadTexture(baseMaterialId, textureBanks);
                    var normalMaterialId = LoadMaterial(materialGuid.Item1, "normalmap", materialBanks);
                    if (normalMaterialId == null)
                        normalMaterialId = LoadMaterial(materialGuid.Item1, "Fur_normalmap", materialBanks);
                    if (normalMaterialId == null)
                        normalMaterialId = LoadMaterial(materialGuid.Item1, "IrisNormal", materialBanks);
                    var normalTexture = LoadTexture(normalMaterialId, textureBanks);
                    var mraoMaterialId = LoadMaterial(materialGuid.Item1, "physicalmap", materialBanks);
                    var mraoTexture = LoadTexture(mraoMaterialId, textureBanks);
                    var hmvyMaterialId = LoadMaterial(materialGuid.Item1, "HMVY", materialBanks);
                    var hmvyTexture = LoadTexture(hmvyMaterialId, textureBanks);
                    var cleaMaterialId = LoadMaterial(materialGuid.Item1, "CLEA", materialBanks);
                    var cleaTexture = LoadTexture(cleaMaterialId, textureBanks);
                    slotTypes.TryGetValue(materialGuid.Item2, out string slotType);
                    geometryList.Add(new MeshGeometry3DObject {
                        ObjectId = name,
                        MaterialId = materialGuid.Item1,
                        BaseMaterialId = baseMaterialId,
                        BaseMap = baseTexture,
                        NormalMaterialId = normalMaterialId,
                        NormalMap = normalTexture,
                        MRAOMaterialId = mraoMaterialId,
                        MRAOMap = mraoTexture,
                        HMVYMaterialId = hmvyMaterialId,
                        HMVYMap = hmvyTexture,
                        CLEAMaterialId = cleaMaterialId,
                        CLEAMap = cleaTexture,
                        MeshGeometry3D = meshGeometry,
                        SlotType = slotType
                    });
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
        private static Tuple<List<string>, Dictionary<string, Tuple<string, string>>, Dictionary<string, string>> LoadCharacterVisualResources(string id, Dictionary<string, string> characterVisualBanks, Dictionary<string, string> visualBanks)
        {
            var characterVisualResources = new List<string>();
            var materials = new Dictionary<string, Tuple<string, string>>();
            var slotTypes = new Dictionary<string, string>();
            if (id != null)
            {
                characterVisualBanks.TryGetValue(id, out string file);
                if (string.IsNullOrEmpty(file))
                    visualBanks.TryGetValue(id, out file);
                if (file != null)
                {
                    var xml = XDocument.Load(FileHelper.GetPath(file));
                    var characterVisualResource = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" && x.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == id).First();
                    var bodySetVisualId = characterVisualResource.Elements("attribute").SingleOrDefault(x => x.Attribute("id").Value == "BodySetVisual")?.Attribute("value").Value;
                    var bodySetVisual = LoadVisualResource(bodySetVisualId, visualBanks);
                    foreach (var material in LoadMaterials(bodySetVisualId, visualBanks))
                    {
                        materials.Add(material.Key, new Tuple<string, string>(material.Value.Item1, id));
                    }
                    if (bodySetVisual != null)
                        characterVisualResources.Add(bodySetVisual);
                    var slots = characterVisualResource.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Slots").ToList();
                    foreach (var slot in slots)
                    {
                        var visualResourceId = slot.Elements("attribute").SingleOrDefault(a => a.Attribute("id").Value == "VisualResource").Attribute("value").Value;
                        var slotType = slot.Elements("attribute").SingleOrDefault(a => a.Attribute("id").Value == "Slot").Attribute("value").Value;
                        slotTypes.Add(visualResourceId, slotType);
                        foreach (var material in LoadMaterials(visualResourceId, visualBanks))
                        {
                            if (!materials.ContainsKey(material.Key))
                                materials.Add(material.Key, new Tuple<string, string>(material.Value.Item1, visualResourceId));
                        }
                        var visualResource = LoadVisualResource(visualResourceId, visualBanks);
                        if (visualResource != null)
                            characterVisualResources.Add(visualResource);
                    }
                }
            }
            return new Tuple<List<string>, Dictionary<string, Tuple<string, string>>, Dictionary<string, string>>(characterVisualResources, materials, slotTypes);
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
        private static Dictionary<string, Tuple<string, string>> LoadMaterials(string id, Dictionary<string, string> visualBanks)
        {
            var materialIds = new Dictionary<string, Tuple<string, string>>();
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
                        var nodes = children.Elements("node");
                        foreach (XElement node in nodes.Where(node => node.Attribute("id").Value == "Objects"))
                        {
                                var materialId = node.Elements("attribute").Single(a => a.Attribute("id").Value == "MaterialID").Attribute("value").Value;
                                var objectId = node.Elements("attribute").Single(a => a.Attribute("id").Value == "ObjectID").Attribute("value").Value;
                                if (materialId != null)
                                    materialIds.Add(objectId.Split('.')[1], new Tuple<string, string>(materialId, id));
                        }
                    }
                }
            }
            return materialIds;
        }

        /// <summary>
        /// Finds the base material id from the material banks.
        /// </summary>
        /// <param name="id">The material id to match.</param>
        /// <param name="type">The map type.</param>
        /// <param name="materialBanks">The materialbanks lookup.</param>
        /// <returns>The base material id.</returns>
        private static string LoadMaterial(string id, string type, Dictionary<string, string> materialBanks)
        {
            if (id != null)
            {
                materialBanks.TryGetValue(id, out string materialBankFile);
                if (materialBankFile != null)
                {
                    var xml = XDocument.Load(FileHelper.GetPath(materialBankFile));
                    var materialNode = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" && x.Elements("attribute")
                        .Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == id).First();
                    var texture2DParams = materialNode.Descendants().Where(x => x.Name.LocalName == "attribute" && x.Attribute("id").Value == "ParameterName").SingleOrDefault(x => x.Attribute("value").Value == type)?.Parent;
                    if (texture2DParams != null)
                        return texture2DParams.Elements("attribute").SingleOrDefault(a => a.Attribute("id").Value == "ID")?.Attribute("value").Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the given texture file by id from the texture banks.
        /// </summary>
        /// <param name="id">The texture id to match.</param>
        /// <param name="textureBanks">The texturebanks lookup.</param>
        /// <returns>The texture filepath.</returns>
        private static string LoadTexture(string id, Dictionary<string, string> textureBanks)
        {
            if (id != null)
            {
                textureBanks.TryGetValue(id, out string textureBankFile);
                if (textureBankFile != null)
                {
                    var xml = XDocument.Load(FileHelper.GetPath(textureBankFile));
                    var textureResourceNode = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" &&
                        x.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == id).First();
                    var ddsFile = textureResourceNode.Elements("attribute").SingleOrDefault(a => a.Attribute("id").Value == "SourceFile")?.Attribute("value").Value;
                    if (ddsFile == null)
                        return null;
                    return FileHelper.GetPath($"Textures\\{ddsFile}");
                }
            }
            return null;
        }
    }
}
