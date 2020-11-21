/// <summary>
/// The root template helper service.
/// Loads information from various unpacked game assets and organizes them for use.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.Models;
    using bg3_modders_multitool.Models.Races;
    using bg3_modders_multitool.Models.StatStructures;
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
        private List<GameObject> GameObjects;
        private Dictionary<string, Translation> TranslationLookup;
        private readonly string[] Paks = { "Shared","Gustav" };
        private readonly string[] ExcludedData = { "BloodTypes","Data","ItemColor","ItemProgressionNames","ItemProgressionVisuals", "XPData"}; // Not stat structures
        public List<string> GameObjectTypes { get; private set; }
        public List<GameObject> FlatGameObjects { get; private set; }
        public List<Translation> Translations { get; private set; }
        public List<Race> Races { get; private set; }
        public List<StatStructure> StatStructures { get; private set; }

        public RootTemplateHelper()
        {
            ReadTranslations();
            foreach(var pak in Paks)
            {
                ReadRootTemplate(pak);
                ReadRaces(pak);
                ReadData(pak);
            }
            SortRootTemplate();
        }

        private bool ReadData(string pak)
        {
            var dataDir = FileHelper.GetPath($"{pak}\\Public\\{pak}\\Stats\\Generated\\Data");
            if(Directory.Exists(dataDir))
            {
                var dataFiles = Directory.EnumerateFiles(dataDir, "*.txt").Where(file => !ExcludedData.Contains(Path.GetFileNameWithoutExtension(file))).ToList();
                StatStructures = StatStructures ?? new List<StatStructure>();
                foreach(var file in dataFiles)
                {
                    var fileType = StatStructure.FileType(file);
                    var line = string.Empty;
                    var fileStream = new StreamReader(file);
                    while((line = fileStream.ReadLine()) != null)
                    {
                        if(line.Contains("new entry"))
                        {
                            StatStructures.Add(StatStructure.New(fileType));
                        }
                        else if(line.IndexOf("type") == 0)
                        {
                            StatStructures.Last().Type = fileType;
                        }
                    }
                }
                return true;
            }
            return false;
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
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        /// <summary>
        /// Loads game objects of the designated type.
        /// </summary>
        /// <param name="gameObjectType">The game object type to load.</param>
        /// <returns>A collection of game objects.</returns>
        public async Task<ObservableCollection<GameObject>> LoadRelevent(string gameObjectType)
        {
            return await Task.Run(() => {
                var start = DateTime.Now;
                CheckForValidGameObjectType(gameObjectType);
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
                Translations = new List<Translation>();
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
            Application.Current.Dispatcher.Invoke(() => {
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Failed to load english.xml. Please unpack English.pak to generate translations.\n";
            });
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
                GameObjects = GameObjects ?? new List<GameObject>();
                GameObjectTypes = GameObjectTypes ?? new List<string>();
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
                                var value = reader.GetAttribute("value") ?? reader.GetAttribute("handle");
                                if (reader.Depth == 5) // GameObject attributes
                                {
                                    switch (id)
                                    {
                                        case "MapKey":
                                            gameObject.MapKey = value;
                                            break;
                                        case "ParentTemplateId":
                                            gameObject.ParentTemplateId = value;
                                            break;
                                        case "Name":
                                            gameObject.Name = value;
                                            break;
                                        case "DisplayName":
                                            gameObject.DisplayName = TranslationLookup.ContainsKey(value) ? TranslationLookup[value].Value : string.Empty;
                                            gameObject.DisplayNameHandle = value;
                                            break;
                                        case "Description":
                                            gameObject.DescriptionHandle = value;
                                            gameObject.Description = TranslationLookup.ContainsKey(value) ? TranslationLookup[value].Value : string.Empty;
                                            break;
                                        case "Type":
                                            CheckForValidGameObjectType(value);
                                            gameObject.Type = value;
                                            break;
                                        case "Icon":
                                            gameObject.Icon = value;
                                            break;
                                        case "Stats":
                                            gameObject.Stats = value;
                                            break;
                                        case "Race":
                                            gameObject.RaceUUID = value;
                                            break;
                                    }
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
            Application.Current.Dispatcher.Invoke(() => {
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Failed to load root template _merged.lsf for {pak}.pak.\n";
            });
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
                var lookup = GameObjects.GroupBy(go => go.MapKey, StringComparer.OrdinalIgnoreCase).ToDictionary(go => go.Key, go => go.Last());
                foreach (var gameObject in children)
                {
                    lookup[gameObject.ParentTemplateId].Children.Add(gameObject);
                }
                GameObjects = GameObjects.Where(go => string.IsNullOrEmpty(go.ParentTemplateId)).ToList();
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
                Races = Races ?? new List<Race>();
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
            Application.Current.Dispatcher.Invoke(() => {
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += $"Failed to load Races.lsx for {pak}.pak.\n";
            });
            return false;
        }

        /// <summary>
        /// Checks for game object types and forces them to be accounted for.
        /// </summary>
        /// <param name="type">The game object type.</param>
        private void CheckForValidGameObjectType(string type)
        {
            if (!GameObjectTypes.Contains(type))
            {
                GameObjectTypes.Add(type);
            }
        }
        #endregion
    }
}
