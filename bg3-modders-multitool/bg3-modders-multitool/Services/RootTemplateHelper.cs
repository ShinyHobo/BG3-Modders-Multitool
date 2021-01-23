/// <summary>
/// The root template helper service.
/// Loads information from various unpacked game assets and organizes them for use.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.Enums;
    using bg3_modders_multitool.Models;
    using bg3_modders_multitool.Models.Races;
    using bg3_modders_multitool.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml;

    public class RootTemplateHelper
    {
        private List<GameObject> GameObjects = new List<GameObject>();
        private Dictionary<string, Translation> TranslationLookup;
        private readonly string[] Paks = { "Shared","Gustav" };
        private readonly string[] ExcludedData = { "BloodTypes","Data","ItemColor","ItemProgressionNames","ItemProgressionVisuals", "XPData"}; // Not stat structures
        public List<GameObjectType> GameObjectTypes { get; private set; } = new List<GameObjectType>();
        public List<GameObject> FlatGameObjects { get; private set; } = new List<GameObject>();
        public List<Translation> Translations { get; private set; } = new List<Translation>();
        public List<Race> Races { get; private set; } = new List<Race>();
        public List<Models.StatStructures.StatStructure> StatStructures { get; private set; } = new List<Models.StatStructures.StatStructure>();
        public List<TextureAtlas> TextureAtlases { get; private set; } = new List<TextureAtlas>();
        public Dictionary<string, string> GameObjectAttributes { get; set; } = new Dictionary<string,string>();

        public RootTemplateHelper()
        {
            GameObjectTypes = Enum.GetValues(typeof(GameObjectType)).Cast<GameObjectType>().ToList();
            ReadTranslations();
            foreach(var pak in Paks)
            {
                ReadRootTemplate(pak);
                ReadData(pak);
                ReadIcons(pak);
            }
            if(!TextureAtlases.Any(ta => ta.AtlasImage != null)) // no valid textures found
            {
                GeneralHelper.WriteToConsole($"No valid texture atlases found. Unpack Icons.pak to generate icons.\n");
            }
            ReadRaces("Shared");
            SortRootTemplate();
            var attributeValueTypes = string.Join(",", GameObjectAttributes.Values.GroupBy(g => g).Select(g => g.Last()).ToList());
        }

        /// <summary>
        /// Forces garbage collection.
        /// </summary>
        public void Clear()
        {
            GameObjects.Clear();
            TranslationLookup.Clear();
            FlatGameObjects.Clear();
            Translations.Clear();
            Races.Clear();
            StatStructures.Clear();
            TextureAtlases.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
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
            GeneralHelper.WriteToConsole($"Failed to load english.xml. Please unpack English.pak to generate translations.\n");
            return false;
        }

        /// <summary>
        /// Reads the root template and converts it into a GameObject list.
        /// </summary>
        /// <param name="pak">The pak to search in.</param>
        /// <returns>Whether the root template was read.</returns>
        private bool ReadRootTemplate(string pak)
        {
            var rootTemplates = $"{pak}\\Public\\{pak}\\RootTemplates\\_merged.lsf";
            if (File.Exists(FileHelper.GetPath(rootTemplates)))
            {
                rootTemplates = FileHelper.GetPath(FileHelper.Convert(rootTemplates,"lsx"));
                using (XmlReader reader = XmlReader.Create(rootTemplates))
                {
                    GameObject gameObject = null;

                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                var id = reader.GetAttribute("id");
                                if (id == "GameObjects")
                                {
                                    gameObject = new GameObject { Pak = pak, Children = new List<GameObject>() };
                                }
                                var type = reader.GetAttribute("type");
                                var value = reader.GetAttribute("value") ?? reader.GetAttribute("handle");
                                if (reader.Depth == 5) // GameObject attributes
                                {
                                    gameObject.LoadProperty(id, type, value);

                                    if (id != null && !GameObjectAttributes.ContainsKey(id))
                                        GameObjectAttributes.Add(id, type);
                                }
                                break;
                            case XmlNodeType.EndElement:
                                if (reader.Depth == 4) // end of GameObject
                                {
                                    GameObjects.Add(gameObject);
                                }
                                break;
                        }
                    }
                }
                return true;
            }
            GeneralHelper.WriteToConsole($"Failed to load root template _merged.lsf for {pak}.pak.\n");
            return false;
        }

        /// <summary>
        /// Groups children by MapKey and ParentTemplateId
        /// </summary>
        /// <returns>Whether the GameObjects list was sorted.</returns>
        private bool SortRootTemplate()
        {
            if(GameObjects != null)
            {
                GameObjectTypes = GameObjectTypes.OrderBy(got => got).ToList();
                GameObjects = GameObjects.OrderBy(go => go.Name).ToList();
                FlatGameObjects = GameObjects;
                var children = GameObjects.Where(go => !string.IsNullOrEmpty(go.ParentTemplateId)).ToList();
                var lookup = GameObjects.GroupBy(go => go.MapKey).ToDictionary(go => go.Key, go => go.Last());
                foreach (var gameObject in children)
                {
                    lookup.First(l => l.Key == gameObject.ParentTemplateId).Value.Children.Add(gameObject);
                }
                GameObjects = GameObjects.Where(go => string.IsNullOrEmpty(go.ParentTemplateId)).ToList();
                foreach(var gameObject in GameObjects)
                {
                    gameObject.PassOnStats();
                }
                return true;
            }
            return false;
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
                                            race.UUID = value;
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
                    var fileStream = new StreamReader(file);
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
        #endregion
    }
}
