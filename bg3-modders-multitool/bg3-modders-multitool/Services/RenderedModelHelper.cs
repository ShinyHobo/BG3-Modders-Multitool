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
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using LSLib.Granny.Model;

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
                    if(characterVisualResources == null)
                    {
                        break;
                    }
                    slotTypes = characterVisualResources.Item3;
                    materials = characterVisualResources.Item2;
                    gr2Files.AddRange(characterVisualResources.Item1);
                    break;
                case "item":
                case "scenery":
                case "TileConstruction":
                    try
                    {
                        materials = LoadMaterials(template, visualBanks);
                        var itemVisualResource = LoadVisualResource(template, visualBanks);
                        if (itemVisualResource != null)
                        {
                            gr2Files.Add(itemVisualResource);
                        }
                    }
                    catch(Exception ex)
                    {
                        GeneralHelper.WriteToConsole($"{ex.Message}\n{ex.StackTrace}");
                    }
                    break;
                default:
                    break;
            }

            var geometryGroup = new List<MeshGeometry>();

            try
            {
                foreach(var gr2File in gr2Files)
                {
                    try
                    {
                        var geometry = GetMesh(gr2File, materials, slotTypes, materialBanks, textureBanks);
                        if (geometry != null)
                        {
                            geometryGroup.Add(new MeshGeometry(gr2File.Replace($"{FileHelper.UnpackedDataPath}\\", string.Empty).Replace('/', '\\'), geometry));
                        }
                    }
                    catch (Exception ex)
                    {
                        GeneralHelper.WriteToConsole($"{ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
            catch (Exception ex)
            {
                GeneralHelper.WriteToConsole($"{ex.Message}\n{ex.StackTrace}");
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
            PakReaderHelper.OpenPakFile(filename + ".GR2");
            var file = LoadFile(filename);
            if (file == null)
                return null;

            // Gather meshes
            var meshes = file.Root.Items.Where(x => x.Items.Any(y => y as MeshNode != null)).ToList();
            // Group meshes by lod
            var meshGroups = meshes.GroupBy(mesh => mesh.Name.Split('_').Last()).ToList();
            var geometryLookup = new Dictionary<string, List<MeshGeometry3DObject>>();

            Parallel.ForEach(meshGroups, GeneralHelper.ParallelOptions, meshGroup =>
            {
                try
                {
                    var geometryList = new List<MeshGeometry3DObject>();

                    // Selecting body first
                    Parallel.ForEach(meshGroup, GeneralHelper.ParallelOptions, mesh =>
                    {
                        try
                        {
                            var name = mesh.Name.Split('-').First();
                            Tuple<string, string> materialGuid = null;
                            if (materials != null && materials.ContainsKey(name))
                                materialGuid = materials[name];
                            var meshNode = mesh.Items.Last() as MeshNode;
                            var meshGeometry = meshNode.Geometry as MeshGeometry3D;
                            var baseMaterialId = LoadMaterial(materialGuid?.Item1, "basecolor", materialBanks);
                            if (baseMaterialId == null)
                                baseMaterialId = LoadMaterial(materialGuid?.Item1, "Body_color_texture", materialBanks);
                            var baseTexture = LoadTexture(baseMaterialId, textureBanks);
                            var normalMaterialId = LoadMaterial(materialGuid?.Item1, "normalmap", materialBanks);
                            if (normalMaterialId == null)
                                normalMaterialId = LoadMaterial(materialGuid?.Item1, "Fur_normalmap", materialBanks);
                            if (normalMaterialId == null)
                                normalMaterialId = LoadMaterial(materialGuid?.Item1, "IrisNormal", materialBanks);
                            var normalTexture = LoadTexture(normalMaterialId, textureBanks);
                            var mraoMaterialId = LoadMaterial(materialGuid?.Item1, "physicalmap", materialBanks);
                            var mraoTexture = LoadTexture(mraoMaterialId, textureBanks);
                            var hmvyMaterialId = LoadMaterial(materialGuid?.Item1, "HMVY", materialBanks);
                            var hmvyTexture = LoadTexture(hmvyMaterialId, textureBanks);
                            var cleaMaterialId = LoadMaterial(materialGuid?.Item1, "CLEA", materialBanks);
                            var cleaTexture = LoadTexture(cleaMaterialId, textureBanks);
                            string slotType = null;
                            if (materialGuid != null && slotTypes.ContainsKey(materialGuid.Item2))
                                slotType = slotTypes[materialGuid.Item2];

                            lock (geometryList)
                                geometryList.Add(new MeshGeometry3DObject
                                {
                                    ObjectId = name,
                                    MaterialId = materialGuid?.Item1,
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
                        catch (Exception ex)
                        {
                            GeneralHelper.WriteToConsole($"{ex.Message}\n{ex.StackTrace}");
                        }
                    });
                    lock (geometryLookup)
                        geometryLookup.Add(meshGroup.Key, geometryList);
                }
                catch (Exception ex)
                {
                    GeneralHelper.WriteToConsole($"{ex.Message}\n{ex.StackTrace}");
                }
            });
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
            var gr2 = $"{filename}.GR2";

            if (!File.Exists(dae) && File.Exists("\\\\?\\" + gr2))
            {
                GeneralHelper.WriteToConsole(Properties.Resources.ConvertingModelDae);
                try
                {
                    var exporter = new LSLib.Granny.Model.Exporter();
                    exporter.Options = new ExporterOptions { InputFormat = ExportFormat.GR2, OutputFormat = ExportFormat.DAE };
                    var original = LSLib.Granny.GR2Utils.LoadModel("\\\\?\\" + gr2, exporter.Options);
                    LSLib.Granny.GR2Utils.SaveModel(original, "\\\\?\\"+dae, exporter);
                }
                catch (Exception ex)
                {
                    if(ex.Message.Contains("Granny2.dll"))
                    {
                        GeneralHelper.WriteToConsole(Properties.Resources.Granny2Error);
                    }
                    else
                    {
                        GeneralHelper.WriteToConsole(ex.Message);
                    }
                    return null;
                }
            }
            try
            {
                var importer = new Importer();
                // Update material here?
                if(File.Exists("\\\\?\\" + dae))
                {
                    var file = importer.Load("\\\\?\\" + dae);
                    if (file == null && File.Exists("\\\\?\\" + dae))
                    {
                        GeneralHelper.WriteToConsole(Properties.Resources.FixingVerticies);
                        try
                        {
                            var xml = XDocument.Load(dae);
                            var geometryList = xml.Descendants().Where(x => x.Name.LocalName == "geometry").ToList();
                            Parallel.ForEach(geometryList, GeneralHelper.ParallelOptions, lod =>
                            {
                                var vertexId = lod.Descendants().Where(x => x.Name.LocalName == "vertices").Select(x => x.Attribute("id").Value).First();
                                var vertex = lod.Descendants().Single(x => x.Name.LocalName == "input" && x.Attribute("semantic").Value == "VERTEX");
                                vertex.Attribute("source").Value = $"#{vertexId}";
                            });
                            xml.Save(dae);
                            GeneralHelper.WriteToConsole(Properties.Resources.ModelConversionComplete);
                            file = importer.Load(dae);
                        }
                        catch (Exception ex)
                        {
                            // in use by another process
                            GeneralHelper.WriteToConsole(ex.Message);
                        }
                    }

                    if (!File.Exists($"\\\\?\\{filename}.fbx"))
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
                GeneralHelper.WriteToConsole(Properties.Resources.FailedToLoadModel, filename);
                importer.Dispose();
                return null;
            }
            catch(Exception ex)
            {
                GeneralHelper.WriteToConsole(Properties.Resources.FailedToLoadDae, ex.Message, ex.InnerException?.Message);
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
        private static Tuple<List<string>, Dictionary<string, Tuple<string, string>>, Dictionary<string, string>>
            LoadCharacterVisualResources(string id, Dictionary<string, string> characterVisualBanks, Dictionary<string, string> visualBanks)
        {
            if (id == null)
                return null;
            var characterVisualResources = new List<string>();
            var materials = new Dictionary<string, Tuple<string, string>>();
            var slotTypes = new Dictionary<string, string>();
            var visualBanksHasKey = visualBanks.ContainsKey(id);
            var characterVisualBanksHasKey = characterVisualBanks.ContainsKey(id);
            if (visualBanksHasKey || characterVisualBanksHasKey)
            {
                var bankFile = characterVisualBanksHasKey ? characterVisualBanks[id] : visualBanks[id];
                try
                {
                    if(bankFile != null)
                    {
                        var file = FileHelper.GetPath(bankFile);
                        if(File.Exists(file))
                        {
                            using (XmlReader reader = XmlReader.Create(file))
                            {
                                reader.MoveToContent();
                                while (!reader.EOF)
                                {
                                    if (reader.Name == "region")
                                    {
                                        var sectionId = reader.GetAttribute("id");
                                        var isCharacterVisualBank = sectionId == "CharacterVisualBank";
                                        var isVisualBank = sectionId == "VisualBank";
                                        if(isCharacterVisualBank || isVisualBank)
                                        {
                                            if (!reader.ReadToDescendant("children"))
                                            {
                                                reader.ReadToFollowing("region");
                                            }

                                            reader.ReadToDescendant("node");
                                            do
                                            {
                                                reader.ReadToDescendant("attribute");
                                                var resourceId = string.Empty;
                                                var bodySetVisualId = string.Empty;
                                                do
                                                {
                                                    var attributeId = reader.GetAttribute("id");
                                                    if(attributeId != null)
                                                    {
                                                        if (attributeId == "ID")
                                                        {
                                                            resourceId = reader.GetAttribute("value");

                                                            if (!string.IsNullOrEmpty(bodySetVisualId) && resourceId == id)
                                                            {
                                                                var bodySetVisualResource = LoadVisualResource(bodySetVisualId, visualBanks);

                                                                foreach (var material in LoadMaterials(bodySetVisualId, visualBanks))
                                                                {
                                                                    if (material.Key != null)
                                                                        materials.Add(material.Key, new Tuple<string, string>(material.Value.Item1, id));
                                                                }

                                                                if (bodySetVisualResource != null)
                                                                    characterVisualResources.Add(bodySetVisualResource);

                                                                break;
                                                            }
                                                        }
                                                        else if (attributeId == "BodySetVisual")
                                                        {
                                                            bodySetVisualId = reader.GetAttribute("value");
                                                        }
                                                    }
                                                } while (reader.ReadToNextSibling("attribute"));

                                                if (resourceId == id)
                                                {
                                                    reader.ReadToNextSibling("children");
                                                    reader.ReadToDescendant("node");
                                                    do
                                                    {
                                                        var nodeId = reader.GetAttribute("id");
                                                        if (nodeId == "Slots")
                                                        {
                                                            var visualResourceId = string.Empty;
                                                            var slotType = string.Empty;
                                                            reader.ReadToDescendant("attribute");
                                                            do
                                                            {
                                                                var attributeId = reader.GetAttribute("id");
                                                                if (attributeId == "VisualResource")
                                                                {
                                                                    visualResourceId = reader.GetAttribute("value");
                                                                }
                                                                else if (attributeId == "Slot")
                                                                {
                                                                    slotType = reader.GetAttribute("value");
                                                                }
                                                            } while (reader.ReadToNextSibling("attribute"));

                                                            foreach (var material in LoadMaterials(bodySetVisualId, visualBanks))
                                                            {
                                                                if (material.Key != null)
                                                                    materials.Add(material.Key, new Tuple<string, string>(material.Value.Item1, id));
                                                            }

                                                            var visualResource = LoadVisualResource(visualResourceId, visualBanks);
                                                            if (visualResource != null)
                                                                characterVisualResources.Add(visualResource);
                                                            slotTypes.Add(visualResourceId, slotType);
                                                        }
                                                    } while (reader.ReadToNextSibling("node"));
                                                    return new Tuple<List<string>, Dictionary<string, Tuple<string, string>>, Dictionary<string, string>>(characterVisualResources, materials, slotTypes);
                                                }
                                            } while (reader.ReadToNextSibling("node"));
                                        }
                                    }
                                    reader.ReadToFollowing("region");
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.FailedToLoadFile, bankFile, ex.Message);
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds the visualresource sourcefile for an object.
        /// </summary>
        /// <param name="id">The id to match.</param>
        /// <param name="visualBanks">The visualbanks lookup.</param>
        /// <returns>The .GR2 sourcefile.</returns>
        private static string LoadVisualResource(string id, Dictionary<string, string> visualBanks)
        {
            if (id != null && visualBanks.ContainsKey(id))
            {
                var visualResourceFile = visualBanks[id];
                visualResourceFile = FileHelper.GetPath(visualResourceFile);
                if(File.Exists(visualResourceFile))
                {
                    try
                    {
                        using (XmlReader reader = XmlReader.Create(visualResourceFile))
                        {
                            reader.MoveToContent();
                            while (!reader.EOF)
                            {
                                if (reader.Name == "region")
                                {
                                    var sectionId = reader.GetAttribute("id");
                                    if (!reader.ReadToDescendant("children"))
                                    {
                                        reader.ReadToFollowing("region");
                                    }

                                    reader.ReadToDescendant("node");
                                    do
                                    {
                                        reader.ReadToDescendant("attribute");
                                        var resourceId = string.Empty;
                                        var gr2File = string.Empty;
                                        do
                                        {
                                            var attributeId = reader.GetAttribute("id");
                                            if (attributeId == "ID")
                                            {
                                                resourceId = reader.GetAttribute("value");
                                            }
                                            else if (attributeId == "SourceFile" && resourceId == id)
                                            {
                                                gr2File = reader.GetAttribute("value");
                                                if (string.IsNullOrEmpty(gr2File))
                                                    return null;
                                                gr2File = gr2File.Replace(".GR2", string.Empty);
                                                return FileHelper.GetPath($"Models\\{gr2File}");
                                            }
                                        } while (reader.ReadToNextSibling("attribute"));
                                    } while (reader.ReadToNextSibling("node"));
                                }
                                reader.ReadToFollowing("region");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GeneralHelper.WriteToConsole(Properties.Resources.FailedToLoadFile, visualResourceFile, ex.Message);
                        return null;
                    }
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
            if (id != null && visualBanks.ContainsKey(id) && false)
            {
                var visualResourceFile = visualBanks[id];
                var xml = XDocument.Load(FileHelper.GetPath(visualResourceFile));
                var visualResourceNode = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" && x.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == id).First();
                var children = visualResourceNode.Element("children");
                if (children != null)
                {
                    var nodes = children.Elements("node");
                    Parallel.ForEach(nodes.Where(node => node.Attribute("id").Value == "Objects"), GeneralHelper.ParallelOptions, node =>
                    {
                        var materialId = node.Elements("attribute").Single(a => a.Attribute("id").Value == "MaterialID").Attribute("value").Value;
                        var objectId = node.Elements("attribute").Single(a => a.Attribute("id").Value == "ObjectID").Attribute("value").Value;
                        if (materialId != null)
                            materialIds.Add(objectId.Split('.')[1], new Tuple<string, string>(materialId, id));
                    });
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
            if(id != null && materialBanks.ContainsKey(id) && false)
            {
                var materialBankFile = materialBanks[id];
                var xml = XDocument.Load(FileHelper.GetPath(materialBankFile));
                var materialNode = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" && x.Elements("attribute")
                    .Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == id).First();
                var texture2DParams = materialNode.Descendants().Where(x => x.Name.LocalName == "attribute" && x.Attribute("id").Value == "ParameterName").SingleOrDefault(x => x.Attribute("value").Value == type)?.Parent;
                if (texture2DParams != null)
                    return texture2DParams.Elements("attribute").SingleOrDefault(a => a.Attribute("id").Value == "ID")?.Attribute("value").Value;
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
            if(id != null && textureBanks.ContainsKey(id) && false)
            {
                var textureBankFile = textureBanks[id];
                var xml = XDocument.Load(FileHelper.GetPath(textureBankFile));
                var textureResourceNode = xml.Descendants().Where(x => x.Name.LocalName == "node" && x.Attribute("id").Value == "Resource" &&
                    x.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value == id).First();
                var ddsFile = textureResourceNode.Elements("attribute").SingleOrDefault(a => a.Attribute("id").Value == "SourceFile")?.Attribute("value").Value;
                if (ddsFile == null)
                    return null;
                return FileHelper.GetPath($"Textures\\{ddsFile}");
            }
            return null;
        }
    }
}
