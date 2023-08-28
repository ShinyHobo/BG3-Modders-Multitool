/// <summary>
/// The root template helper service.
/// Loads information from various unpacked game assets and organizes them for use.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Enums;
    using bg3_modders_multitool.Models;
    using bg3_modders_multitool.Models.Races;
    using bg3_modders_multitool.Properties;
    using LSLib.LS;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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
        public Dictionary<string, string> MaterialBanks { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> TextureBanks { get; set; } = new Dictionary<string, string>();

        public RootTemplateHelper(ViewModels.GameObjectViewModel gameObjectViewModel)
        {
            GeneralHelper.WriteToConsole(Properties.Resources.OpeningGOE);
            var start = DateTime.Now;
            var rootTemplateTask = LoadRootTemplates();

            rootTemplateTask.ContinueWith(t => {
                var timePassed = DateTime.Now.Subtract(start).TotalSeconds;
                GeneralHelper.WriteToConsole(Properties.Resources.FailedGOE, timePassed);
                GeneralHelper.WriteToConsole(t.Exception.Message);
                foreach(var ex in t.Exception.InnerExceptions)
                {
                    GeneralHelper.WriteToConsole(ex.Message);
                    GeneralHelper.WriteToConsole(ex.StackTrace);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

            rootTemplateTask.ContinueWith(delegate {
                if(Loaded)
                {
                    if(GameObjects.Count == 0)
                    {
                        GeneralHelper.WriteToConsole(Properties.Resources.NoGameObjectsFound);
                        System.Windows.Application.Current.Dispatcher.Invoke(() => {
                            gameObjectViewModel.View.Close();
                        });
                    }
                    else
                    {
                        var timePassed = DateTime.Now.Subtract(start).TotalSeconds;
                        GeneralHelper.WriteToConsole(Properties.Resources.LoadedGOE, timePassed);
                        gameObjectViewModel.Loaded = true;
                    }
                }
                else
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.CancelledGOE);
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
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

                // check if Models directory exists
                if(!Directory.Exists($"{Directory.GetCurrentDirectory()}\\UnpackedData\\Models"))
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.FailedToFindModelsPak);
                }

                ReadTranslations();
                ReadVisualBanks();
                // ReadTextureBanks();
                ReadRootTemplate();
                foreach (var pak in Paks)
                {
                    ReadData(pak);
                    ReadIcons(pak);
                }
                if (!TextureAtlases.Any(ta => ta.AtlasImage != null)) // no valid textures found
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.FailedToFindIconsPak);
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
            var translationFile = FileHelper.GetPath(@"English\Localization\English\english.loca");
            var translationFileConverted = FileHelper.GetPath(@"English\Localization\English\english.xml");
            if (!File.Exists(translationFileConverted) && File.Exists(translationFile))
            {
                using (var fs = File.Open(translationFile, System.IO.FileMode.Open))
                {
                    var resource = LocaUtils.Load(fs, LocaFormat.Loca);
                    LocaUtils.Save(resource, translationFileConverted, LocaFormat.Xml);
                }
            }

            if (File.Exists(translationFileConverted))
            {
                if (!FileHelper.TryParseXml(translationFileConverted))
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.CorruptXmlFile, translationFileConverted);
                }
                else
                {
                    using (XmlReader reader = XmlReader.Create(translationFileConverted))
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
                        GeneralHelper.WriteToConsole(Properties.Resources.TranslationsLoaded);
                        return true;
                    }
                }
            }
            GeneralHelper.WriteToConsole(Properties.Resources.FailedToFindEnglishPak);
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

            GeneralHelper.WriteToConsole(Properties.Resources.ReadingGameObjects);
            var rootTemplates = GetFileList("GameObjects");
            var typeBag = new ConcurrentBag<string>();
            #if DEBUG
            var idBag = new ConcurrentBag<string>();
            var classBag = new ConcurrentBag<Tuple<string, string>>();
            #endif
            Parallel.ForEach(rootTemplates, GeneralHelper.ParallelOptions, rootTemplate =>
            {
                rootTemplate = FileHelper.GetPath(rootTemplate);
                if (File.Exists(rootTemplate))
                {
                    var rootTemplatePath = FileHelper.Convert(rootTemplate, "lsx", rootTemplate.Replace(".lsf", ".lsx"));
                    if(File.Exists(rootTemplatePath))
                    {
                        var fileLocation = rootTemplatePath.Replace($"{Directory.GetCurrentDirectory()}\\UnpackedData\\", string.Empty);
                        if (!FileHelper.TryParseXml(rootTemplatePath))
                        {
                            GeneralHelper.WriteToConsole(Properties.Resources.CorruptXmlFile, fileLocation);
                            return;
                        }

                        var pak = Regex.Match(rootTemplatePath, @"(?<=UnpackedData\\).*?(?=\\)").Value;
                        var stream = File.OpenText(rootTemplatePath);

                        using (var fileStream = stream)
                        using (var reader = new XmlTextReader(fileStream))
                        {
                            reader.Read();
                            while (!reader.EOF)
                            {
                                if (reader.NodeType == XmlNodeType.Element && reader.IsStartElement() && reader.GetAttribute("id") == "GameObjects")
                                {
                                    var xml = (XElement)XNode.ReadFrom(reader);
                                    var gameObject = new GameObject { Pak = pak, Children = new List<GameObject>(), FileLocation = fileLocation };
                                    var attributes = xml.Elements("attribute");

                                    foreach (XElement attribute in attributes)
                                    {
                                        var id = attribute.Attribute("id").Value;
                                        var handle = attribute.Attribute("handle")?.Value;
                                        var value = handle ?? attribute.Attribute("value").Value;
                                        var type = attribute.Attribute("type").Value;
                                        if (int.TryParse(type, out int typeInt))
                                            type = GeneralHelper.LarianTypeEnumConvert(type);

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
                                            if(value != null && TranslationLookup.ContainsKey(value))
                                            {
                                                var translationText = TranslationLookup[value].Value;
                                                gameObject.LoadProperty(id, type, translationText);
                                            }
                                        }
                                    }

                                    if (string.IsNullOrEmpty(gameObject.ParentTemplateId))
                                        gameObject.ParentTemplateId = gameObject.TemplateName;
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
                }
            });
            #if DEBUG
            FileHelper.SerializeObject(typeBag.ToList().Distinct().ToList(), "GameObjectTypes");
            FileHelper.SerializeObject(idBag.ToList().Distinct().ToList(), "GameObjectAttributeIds");
            GeneralHelper.ClassBuilder(classBag.ToList().Distinct().ToList());
            #endif

            if(GameObjectBag.Count == 0)
            {
                return false;
            }

            GeneralHelper.WriteToConsole(Properties.Resources.GameObjectsLoaded);
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
            if (GameObjectBag.Count == 0)
                return false;
            GeneralHelper.WriteToConsole(Properties.Resources.SortingGameObjects);
            GameObjects = GameObjectBag.OrderBy(go => string.IsNullOrEmpty(go.Name)).ThenBy(go => go.Name).ToList();
            var children = GameObjects.Where(go => !string.IsNullOrEmpty(go.ParentTemplateId)).ToList();
            var orderedChildren = children.AsParallel().WithDegreeOfParallelism(GeneralHelper.ParallelOptions.MaxDegreeOfParallelism).OrderBy(go => string.IsNullOrEmpty(go.Name)).ThenBy(go => go.Name);
            var lookup = GameObjects.Where(go => !string.IsNullOrEmpty(go.MapKey)).GroupBy(go => go.MapKey).ToDictionary(go => go.Key, go => go.Last());
            Parallel.ForEach(orderedChildren, GeneralHelper.ParallelOptions, gameObject =>
            {
                if(lookup.ContainsKey(gameObject.ParentTemplateId))
                {
                    var goChildren = lookup[gameObject.ParentTemplateId].Children;
                    if (goChildren != null)
                    {
                        lock (goChildren)
                            goChildren.Add(gameObject);
                    }
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
                if (!FileHelper.TryParseXml(raceFile))
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.CorruptXmlFile, raceFile);
                    return false;
                }

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
            GeneralHelper.WriteToConsole(Properties.Resources.FailedToLoadRaces, pak);
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
                    using(var fileStream = new System.IO.StreamReader(file))
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
                try
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
                catch
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.CorruptXmlFile, metaLoc);
                }
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
            var deserializedMaterialBanks = FileHelper.DeserializeObject<Dictionary<string, string>>("MaterialBanks");
            var deserializedTextureBanks = FileHelper.DeserializeObject<Dictionary<string, string>>("TextureBanks");

            if (deserializedVisualBanks != null && deserializedCharacterVisualBanks != null && deserializedBodySetVisuals != null && deserializedMaterialBanks != null && deserializedTextureBanks != null)
            {
                CharacterVisualBanks = deserializedCharacterVisualBanks;
                VisualBanks = deserializedVisualBanks;
                BodySetVisuals = deserializedBodySetVisuals;
                MaterialBanks = deserializedMaterialBanks;
                TextureBanks = deserializedTextureBanks;
                return true;
            }

            GeneralHelper.WriteToConsole(Resources.LoadingBankFiles);

            // Lookup CharacterVisualBank file from CharacterVisualResourceID
            var characterVisualBanks = new ConcurrentDictionary<string, string>();
            var visualBanks = new ConcurrentDictionary<string, string>();
            var bodySetVisuals = new ConcurrentDictionary<string, string>();
            var materialBanks = new ConcurrentDictionary<string, string>();
            var textureBanks = new ConcurrentDictionary<string, string>();
            var visualBankFiles = GetFileList("VisualBank");
            if(visualBankFiles.Count > 0)
                GeneralHelper.WriteToConsole(Resources.FoundVisualBanks);
            var materialBankFiles = GetFileList("MaterialBank");
            if(materialBankFiles.Count > 0)
                GeneralHelper.WriteToConsole(Resources.FoundMaterialBanks);
            var textureBankFiles = GetFileList("TextureBank");
            if(textureBankFiles.Count > 0)
                GeneralHelper.WriteToConsole(Resources.FoundTextureBanks);
            visualBankFiles.AddRange(materialBankFiles);
            visualBankFiles.AddRange(textureBankFiles);
            visualBankFiles = visualBankFiles.Distinct().ToList();
            if(visualBankFiles.Count > 0)
                GeneralHelper.WriteToConsole(Resources.SortingBanksFiles);
            Parallel.ForEach(visualBankFiles, GeneralHelper.ParallelOptions, visualBankFile => {
                visualBankFile = FileHelper.GetPath(visualBankFile);
                if (File.Exists(visualBankFile))
                {
                    var visualBankFilePath = FileHelper.Convert(visualBankFile, "lsx", visualBankFile.Replace(".lsf", ".lsx"));
                    var filePath = visualBankFilePath.Replace($"\\\\?\\{Directory.GetCurrentDirectory()}\\UnpackedData", string.Empty);

                    if (!FileHelper.TryParseXml(filePath))
                    {
                        var filePath2 = visualBankFilePath.Replace($"{Directory.GetCurrentDirectory()}\\UnpackedData\\", string.Empty);
                        GeneralHelper.WriteToConsole(Resources.CorruptXmlFile, filePath2);
                        return;
                    }

                    var stream = File.OpenText(visualBankFilePath);
                    using (var fileStream = stream)
                    using (var reader = new XmlTextReader(fileStream))
                    {
                        reader.Read();
                        while (!reader.EOF)
                        {
                            try
                            {
                                var sectionId = reader.GetAttribute("id");
                                var isNode = reader.NodeType == XmlNodeType.Element && reader.IsStartElement() && reader.Name == "node";
                                if (isNode && (sectionId == "CharacterVisualBank" || sectionId == "VisualBank"))
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
                                else if(isNode && sectionId == "MaterialBank")
                                {
                                    var xml = (XElement)XNode.ReadFrom(reader);
                                    var children = xml.Element("children");
                                    if (children != null)
                                    {
                                        var nodes = children.Elements("node");
                                        foreach (XElement node in nodes)
                                        {
                                            var id = node.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value;
                                            materialBanks.TryAdd(id, filePath);
                                        }
                                    }
                                    reader.Skip();
                                }
                                else if (isNode && sectionId == "TextureBank")
                                {
                                    var xml = (XElement)XNode.ReadFrom(reader);
                                    var children = xml.Element("children");
                                    if (children != null)
                                    {
                                        var nodes = children.Elements("node");
                                        foreach (XElement node in nodes)
                                        {
                                            var id = node.Elements("attribute").Single(a => a.Attribute("id").Value == "ID").Attribute("value").Value;
                                            textureBanks.TryAdd(id, filePath);
                                        }
                                    }
                                    reader.Skip();
                                }
                                else
                                {
                                    reader.Read();
                                }
                            }
                            catch(Exception ex)
                            {
                                GeneralHelper.WriteToConsole(Properties.Resources.FailedToLoadFile, filePath, ex.Message);
                                break;
                            }
                        }
                        reader.Close();
                    }
                }
            });

            CharacterVisualBanks = characterVisualBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            VisualBanks = visualBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            BodySetVisuals = bodySetVisuals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            MaterialBanks = materialBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            TextureBanks = textureBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            FileHelper.SerializeObject(CharacterVisualBanks, "CharacterVisualBanks");
            FileHelper.SerializeObject(VisualBanks, "VisualBanks");
            FileHelper.SerializeObject(BodySetVisuals, "BodySetVisuals");
            FileHelper.SerializeObject(MaterialBanks, "MaterialBanks");
            FileHelper.SerializeObject(TextureBanks, "TextureBanks");
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
                rtList.AddRange(results.Result.Matches.Where(r => r.EndsWith(".lsf")));
            }).Wait();
            return rtList;
        }

        /// <summary>
        /// Deletes the GameObject cache, if it exists
        /// </summary>
        public static void ClearGameObjectCache()
        {
            var cacheDirectory = "Cache2";
            try
            {
                if (Directory.Exists(cacheDirectory))
                {
                    // check if cache exists
                    var result = System.Windows.Forms.MessageBox.Show(Properties.Resources.GOEDeleteQuestion, 
                        Properties.Resources.GOEClearCacheButton, System.Windows.Forms.MessageBoxButtons.OKCancel);

                    if (result.Equals(System.Windows.Forms.DialogResult.OK))
                    {
                        Directory.Delete(cacheDirectory);
                        GeneralHelper.WriteToConsole(Properties.Resources.GOECacheCleared);
                    }
                }
                else
                {
                    GeneralHelper.WriteToConsole(Properties.Resources.GOENoCache);
                }
            }
            catch
            {
                GeneralHelper.WriteToConsole(Properties.Resources.GOECacheClearFailed);
            }
        }
        #endregion
    }
}
