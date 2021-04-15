/// <summary>
/// The root template helper service.
/// Loads information from various unpacked game assets and organizes them for use.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.Enums;
    using bg3_modders_multitool.Models;
    using bg3_modders_multitool.Models.Races;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    public class RootTemplateHelper
    {
        private List<Translation> Translations = new List<Translation>();
        private readonly string[] Paks = { "Shared","Gustav" };
        private readonly string[] ExcludedData = { "BloodTypes","Data","ItemColor","ItemProgressionNames","ItemProgressionVisuals", "XPData"}; // Not stat structures
        private bool Loaded = false;
        private bool GameObjectsCached = false;
        private ConcurrentBag<GameObject> GameObjectBag = new ConcurrentBag<GameObject>();
        private IndexHelper IndexHelper = new IndexHelper();
        public List<GameObject> GameObjects = new List<GameObject>();
        public List<GameObjectType> GameObjectTypes { get; private set; } = new List<GameObjectType>();
        public Dictionary<string, Translation> TranslationLookup;
        public List<Race> Races { get; private set; } = new List<Race>();
        public List<Models.StatStructures.StatStructure> StatStructures { get; private set; } = new List<Models.StatStructures.StatStructure>();
        public List<TextureAtlas> TextureAtlases { get; private set; } = new List<TextureAtlas>();
        public Dictionary<string, string> GameObjectAttributes { get; set; } = new Dictionary<string,string>();
        public Dictionary<string, string> CharacterVisualBanks { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> VisualBanks { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> BodySetVisuals { get; set; } = new Dictionary<string, string>();

        public RootTemplateHelper(ViewModels.GameObjectViewModel gameObjectViewModel)
        {
            GeneralHelper.WriteToConsole($"Loading GameObjects...\n");
            var start = DateTime.Now;
            LoadRootTemplates().ContinueWith(delegate {
                if(Loaded)
                {
                    var timePassed = DateTime.Now.Subtract(start).TotalSeconds;
                    GeneralHelper.WriteToConsole($"GameObjects loaded in {timePassed} seconds.\n");
                    gameObjectViewModel.Loaded = true;
                }
                else
                {
                    GeneralHelper.WriteToConsole($"GameObjects loading cancelled.\n");
                }
            });
        }

        /// <summary>
        /// Forces garbage collection.
        /// </summary>
        public void Clear()
        {
            GameObjects.Clear();
            TranslationLookup.Clear();
            Translations.Clear();
            Races.Clear();
            StatStructures.Clear();
            TextureAtlases.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public async Task<string> LoadRootTemplates()
        {
            return await Task.Run(() => {
                GameObjectTypes = Enum.GetValues(typeof(GameObjectType)).Cast<GameObjectType>().OrderBy(got => got).ToList();
                ReadTranslations();
                ReadVisualBanks();
                ReadRootTemplate();
                foreach (var pak in Paks)
                {
                    ReadData(pak);
                    ReadIcons(pak);
                }
                if (!TextureAtlases.Any(ta => ta.AtlasImage != null)) // no valid textures found
                {
                    GeneralHelper.WriteToConsole($"No valid texture atlases found. Unpack Icons.pak to generate icons. Skipping...\n");
                }
                SortRootTemplate();
                Loaded = true;
                return string.Join(",", GameObjectAttributes.Values.GroupBy(g => g).Select(g => g.Last()).ToList());
            });
        }

        /// <summary>
        /// Loads game objects of the designated type.
        /// </summary>
        /// <param name="gameObjectType">The game object type to load.</param>
        /// <returns>A collection of game objects.</returns>
        public async Task<ObservableCollection<GameObject>> LoadRelevent(GameObjectType gameObjectType)
        {
            return await Task.Run(() => {
                var start = DateTime.Now;
                var returnObjects = new ObservableCollection<GameObject>(GameObjects.Where(go => go.Type == gameObjectType));
                var timePassed = DateTime.Now.Subtract(start).TotalSeconds;
                return returnObjects;
            });
        }

        #region Private Methods
        /// <summary>
        /// Reads translation file into translation list.
        /// </summary>
        /// <returns>Whether the translation file was read.</returns>
        private bool ReadTranslations()
        {
            TranslationLookup = new Dictionary<string, Translation>();
            var translationFile = FileHelper.GetPath(@"English\Localization\English\english.xml");
            if (File.Exists(translationFile))
            {
                using (XmlReader reader = XmlReader.Create(translationFile))
                {
                    while (reader.Read())
                    {
                        if (reader.Name == "content")
                        {
                            var id = reader.GetAttribute("contentuid");
                            var text = reader.ReadInnerXml();
                            Translations.Add(new Translation { ContentUid = id, Value = text });
                        }
                    }
                    TranslationLookup = Translations.ToDictionary(go => go.ContentUid);
                    return true;
                }
            }
            GeneralHelper.WriteToConsole($"Failed to load english.xml. Please unpack English.pak to generate translations. Skipping...\n");
            return false;
        }

        /// <summary>
        /// Reads the root template and converts it into a GameObject list.
        /// </summary>
        /// <returns>Whether the root template was read.</returns>
        private bool ReadRootTemplate()
        {
            var deserializedGameObjects = FileHelper.DeserializeObject<List<GameObject>>("GameObjects");
            if(deserializedGameObjects != null)
            {
                GameObjects = deserializedGameObjects;
                GameObjectsCached = true;
                return true;
            }
            var rootTemplates = GetFileList("GameObjects");
            var typeBag = new ConcurrentBag<string>();
            #if DEBUG
            var idBag = new ConcurrentBag<string>();
            var classBag = new ConcurrentBag<Tuple<string, string>>();
            #endif
            Parallel.ForEach(rootTemplates, rootTemplate =>
            {
                if (File.Exists(rootTemplate))
                {
                    var rootTemplatePath = FileHelper.Convert(rootTemplate, "lsx", rootTemplate.Replace(".lsf", ".lsx"));
                    var pak = Regex.Match(rootTemplatePath, @"(?<=UnpackedData\\).*?(?=\\)").Value;

                    using(var fileStream = new StreamReader(rootTemplatePath))
                    using(var reader = new XmlTextReader(fileStream))
                    {
                        reader.Read();
                        while(!reader.EOF)
                        {
                            if(reader.NodeType == XmlNodeType.Element && reader.IsStartElement() && reader.GetAttribute("id") == "GameObjects")
                            {
                                var xml = (XElement)XNode.ReadFrom(reader);
                                var gameObject = new GameObject { Pak = pak, Children = new List<GameObject>(), FileLocation = rootTemplatePath.Replace($"\\\\?\\{Directory.GetCurrentDirectory()}\\UnpackedData", string.Empty) };
                                var attributes = xml.Elements("attribute");

                                foreach(XElement attribute in attributes)
                                {
                                    var id = attribute.Attribute("id").Value;
                                    var handle = attribute.Attribute("handle")?.Value;
                                    var value = handle ?? attribute.Attribute("value").Value;
                                    var type = attribute.Attribute("type").Value;
                                    #if DEBUG
                                    typeBag.Add(type);
                                    idBag.Add(id);
                                    classBag.Add(new Tuple<string, string>(id, type));
                                    #endif
                                    if (string.IsNullOrEmpty(handle))
                                    {
                                        gameObject.LoadProperty(id, type, value);
                                    }
                                    else
                                    {
                                        gameObject.LoadProperty($"{id}Handle", type, value);
                                        var translationText = TranslationLookup.FirstOrDefault(tl => tl.Key.Equals(value)).Value?.Value;
                                        gameObject.LoadProperty(id, type, translationText);
                                    }
                                }

                                if(string.IsNullOrEmpty(gameObject.ParentTemplateId))
                                    gameObject.ParentTemplateId = gameObject.TemplateName;
                                if (string.IsNullOrEmpty(gameObject.CharacterVisualResourceID))
                                    gameObject.CharacterVisualResourceID = gameObject.VisualTemplate;
                                if (string.IsNullOrEmpty(gameObject.Name))
                                    gameObject.Name = gameObject.DisplayName;
                                if (string.IsNullOrEmpty(gameObject.Name))
                                    gameObject.Name = gameObject.Stats;

                                GameObjectBag.Add(gameObject);
                                reader.Skip();
                            }
                            else
                            {
                                reader.Read();
                            }
                        }
                        reader.Close();
                    }
                }
            });
            #if DEBUG
            FileHelper.SerializeObject(typeBag.ToList().Distinct().ToList(), "GameObjectTypes");
            FileHelper.SerializeObject(idBag.ToList().Distinct().ToList(), "GameObjectAttributeIds");
            GeneralHelper.ClassBuilder(classBag.ToList().Distinct().ToList());
            #endif
            return true;
        }

        /// <summary>
        /// Groups children by MapKey and ParentTemplateId
        /// </summary>
        /// <returns>Whether the GameObjects list was sorted.</returns>
        private bool SortRootTemplate()
        {
            if (GameObjectsCached)
                return true;
            GeneralHelper.WriteToConsole($"Sorting GameObjects...\n");
            GameObjects = GameObjectBag.OrderBy(go => string.IsNullOrEmpty(go.Name)).ThenBy(go => go.Name).ToList();
            var children = GameObjects.Where(go => !string.IsNullOrEmpty(go.ParentTemplateId)).ToList();
            var lookup = GameObjects.GroupBy(go => go.MapKey).ToDictionary(go => go.Key, go => go.Last());
            Parallel.ForEach(children.AsParallel().OrderBy(go => string.IsNullOrEmpty(go.Name)).ThenBy(go => go.Name), gameObject =>
            {
                var goChildren = lookup.FirstOrDefault(l => l.Key == gameObject.ParentTemplateId).Value?.Children;
                if(goChildren != null)
                {
                    lock (goChildren)
                        goChildren.Add(gameObject);
                }
            });
            GameObjects = GameObjects.Where(go => string.IsNullOrEmpty(go.ParentTemplateId)).ToList();
            foreach(var gameObject in GameObjects)
            {
                gameObject.PassOnStats();
            }
            FileHelper.SerializeObject(GameObjects, "GameObjects");
                
            return true;
        }

        /// <summary>
        /// Reads the Races.lsx file and converts it into a Race list.
        /// </summary>
        /// <param name="pak">The pak to search in.</param>
        /// <returns>Whether the root template was read.</returns>
        private bool ReadRaces(string pak)
        {
            var raceFile = FileHelper.GetPath($"{pak}\\Public\\{pak}\\Races\\Races.lsx");
            if (File.Exists(raceFile))
            {
                using (XmlReader reader = XmlReader.Create(raceFile))
                {
                    Race race = null;
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                var id = reader.GetAttribute("id");
                                if (id == "Race")
                                {
                                    race = new Race { Components = new List<Component>() };
                                }
                                var value = reader.GetAttribute("value");
                                if (reader.Depth == 5) // top level
                                {
                                    switch (id)
                                    {
                                        case "Description":
                                            race.DescriptionHandle = value;
                                            // TODO load description
                                            break;
                                        case "DisplayName":
                                            race.DisplayNameHandle = value;
                                            // TODO load display name
                                            break;
                                        case "Name":
                                            race.Name = value;
                                            break;
                                        case "ParentGuid":
                                            race.ParentGuid = value;
                                            break;
                                        case "ProgressionTableUUID":
                                            race.ProgressionTableUUID = value;
                                            break;
                                        case "UUID":
                                            race.UUID = new Guid(value);
                                            break;
                                    }
                                }
                                if (reader.Depth == 6) // eye colors, hair colors, tags, makeup colors, skin colors, tattoo colors, visuals
                                {
                                    race.Components.Add(new Component { Type = id });
                                }
                                if (reader.Depth == 7) // previous level values
                                {
                                    race.Components.Last().Guid = value;
                                }
                                break;
                            case XmlNodeType.EndElement:
                                if (reader.Depth == 4)
                                {
                                    Races.Add(race);
                                }
                                break;
                        }
                    }
                }
                return true;
            }
            GeneralHelper.WriteToConsole($"Failed to load Races.lsx for {pak}.pak.\n");
            return false;
        }

        /// <summary>
        /// Reads the stats and converts them into a StatStructure list.
        /// Uses reflection to dynamically generate data.
        /// </summary>
        /// <param name="pak">The pak to search in.</param>
        /// <returns>Whether the stats were read.</returns>
        private bool ReadData(string pak)
        {
            var dataDir = FileHelper.GetPath($"{pak}\\Public\\{pak}\\Stats\\Generated\\Data");
            if (Directory.Exists(dataDir))
            {
                var dataFiles = Directory.EnumerateFiles(dataDir, "*.txt").Where(file => !ExcludedData.Contains(Path.GetFileNameWithoutExtension(file))).ToList();
                foreach (var file in dataFiles)
                {
                    var fileType = Models.StatStructures.StatStructure.FileType(file);
                    var line = string.Empty;
                    using(var fileStream = new StreamReader(file))
                    {
                        while ((line = fileStream.ReadLine()) != null)
                        {
                            if (line.Contains("new entry"))
                            {
                                StatStructures.Add(Models.StatStructures.StatStructure.New(fileType, line.Substring(10)));
                            }
                            else if (line.IndexOf("type") == 0)
                            {
                                StatStructures.Last().Type = fileType;
                            }
                            else if (line.IndexOf("using") == 0)
                            {
                                StatStructures.Last().InheritProperties(line, StatStructures);
                            }
                            else if (!string.IsNullOrEmpty(line))
                            {
                                StatStructures.Last().LoadProperty(line);
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads the texture atlas for icon displays.
        /// </summary>
        /// <param name="pak">The pak to load texture atlas for.</param>
        /// <returns>Whether the texture atlas was created.</returns>
        private bool ReadIcons(string pak)
        {
            var metaLoc = FileHelper.GetPath($"{pak}\\Mods\\{pak}\\meta.lsx");
            if (File.Exists(@"\\?\" + metaLoc))
            {
                var meta = DragAndDropHelper.ReadMeta(metaLoc);
                var characterIconAtlas = FileHelper.GetPath($"{pak}\\Public\\{pak}\\GUI\\Generated_{meta.UUID}_Icons.lsx");
                if (File.Exists(@"\\?\" + characterIconAtlas))
                {
                    TextureAtlases.Add(TextureAtlas.Read(characterIconAtlas, pak));
                }
                var objectIconAtlas = FileHelper.GetPath($"{pak}\\Public\\{pak}\\GUI\\Icons_Items.lsx");
                if (File.Exists(@"\\?\" + objectIconAtlas))
                {
                    TextureAtlases.Add(TextureAtlas.Read(objectIconAtlas, pak));
                }
                var objectIconAtlas2 = FileHelper.GetPath($"{pak}\\Public\\{pak}\\GUI\\Icons_Items_2.lsx");
                if (File.Exists(@"\\?\" + objectIconAtlas2))
                {
                    TextureAtlases.Add(TextureAtlas.Read(objectIconAtlas2, pak));
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Reads the visual banks for a list of id/filepath references for quick lookup.
        /// </summary>
        /// <returns>Whether the visual bank lists were created.</returns>
        private bool ReadVisualBanks()
        {
            var deserializedCharacterVisualBanks = FileHelper.DeserializeObject<Dictionary<string,string>>("CharacterVisualBanks");
            var deserializedVisualBanks = FileHelper.DeserializeObject<Dictionary<string, string>>("VisualBanks");
            var deserializedBodySetVisuals = FileHelper.DeserializeObject<Dictionary<string, string>>("BodySetVisuals");

            if (deserializedVisualBanks != null && deserializedCharacterVisualBanks != null && deserializedBodySetVisuals != null)
            {
                CharacterVisualBanks = deserializedCharacterVisualBanks;
                VisualBanks = deserializedVisualBanks;
                BodySetVisuals = deserializedBodySetVisuals;
                return true;
            }

            // Lookup CharacterVisualBank file from CharacterVisualResourceID
            var characterVisualBanks = new ConcurrentDictionary<string, string>();
            var visualBanks = new ConcurrentDictionary<string, string>();
            var bodySetVisuals = new ConcurrentDictionary<string, string>();
            var visualBankFiles = GetFileList("VisualBank");
            Parallel.ForEach(visualBankFiles, visualBankFile => {
                if (File.Exists(visualBankFile))
                {
                    var visualBankFilePath = FileHelper.Convert(visualBankFile, "lsx", visualBankFile.Replace(".lsf", ".lsx"));
                    var filePath = visualBankFilePath.Replace($"\\\\?\\{Directory.GetCurrentDirectory()}\\UnpackedData", string.Empty);

                    using (var fileStream = new StreamReader(visualBankFilePath))
                    using (var reader = new XmlTextReader(fileStream))
                    {
                        reader.Read();
                        while (!reader.EOF)
                        {
                            var sectionId = reader.GetAttribute("id");
                            if (reader.NodeType == XmlNodeType.Element && reader.IsStartElement() && reader.Name == "node" && (sectionId == "CharacterVisualBank" || sectionId == "VisualBank"))
                            {
                                // read children for resource nodes
                                var xml = (XElement)XNode.ReadFrom(reader);
                                var children = xml.Element("children");
                                if (children != null)
                                {
                                    var nodes = children.Elements("node");
                                    foreach (XElement node in nodes)
                                    {
                                        var id = node.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value;
                                        if (sectionId == "CharacterVisualBank")
                                        {
                                            characterVisualBanks.TryAdd(id, filePath);
                                            var bodySetVisual = node.Elements("attribute").Single(a => a.Attribute("id").Value == "BodySetVisual").Attribute("value").Value;
                                            if (bodySetVisual != null)
                                                bodySetVisuals.TryAdd(bodySetVisual, filePath);
                                        }
                                        else
                                        {
                                            visualBanks.TryAdd(id, filePath);
                                        }
                                    }
                                }

                                reader.Skip();
                            }
                            else
                            {
                                reader.Read();
                            }
                        }
                        reader.Close();
                    }
                }
            });

            CharacterVisualBanks = characterVisualBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            VisualBanks = visualBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            BodySetVisuals = bodySetVisuals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            FileHelper.SerializeObject(CharacterVisualBanks, "CharacterVisualBanks");
            FileHelper.SerializeObject(VisualBanks, "VisualBanks");
            FileHelper.SerializeObject(BodySetVisuals, "BodySetVisuals");
            return true;
        }

        /// <summary>
        /// Gets the file list from all unpacked paks containing a certain node.
        /// </summary>
        /// <param name="searchTerm">The term to search on.</param>
        /// <returns>The file list.</returns>
        private List<string> GetFileList(string searchTerm)
        {
            var rtList = new List<string>();
            IndexHelper.SearchFiles(searchTerm, false).ContinueWith(results => {
                rtList.AddRange(results.Result.Where(r => r.EndsWith(".lsf")));
            }).Wait();
            return rtList;
        }
        #endregion
    }
}
