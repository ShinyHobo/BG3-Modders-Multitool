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

    public class RootTemplateHelper
    {
        #region Properties
        private List<Translation> Translations = new List<Translation>();
        public List<PakReaderHelper> PakReaderHelpers { get; private set; } = new List<PakReaderHelper>();
        private readonly string[] ExcludedData = { "BloodTypes","Data","ItemColor","ItemProgressionNames","ItemProgressionVisuals", "XPData"}; // Not stat structures
        private bool Loaded = false;
        private bool GameObjectsCached = false;
        private ConcurrentBag<GameObject> GameObjectBag = new ConcurrentBag<GameObject>();
        private IndexHelper IndexHelper = new IndexHelper();
        private ViewModels.GameObjectViewModel GameObjectViewModel;
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
        #endregion

        public RootTemplateHelper(ViewModels.GameObjectViewModel gameObjectViewModel)
        {
            GeneralHelper.WriteToConsole(Properties.Resources.OpeningGOE);
            var start = DateTime.Now;
            GameObjectViewModel = gameObjectViewModel;
            var rootTemplateTask = LoadRootTemplates();

            rootTemplateTask.ContinueWith(t => {
                var timePassed = DateTime.Now.Subtract(start).TotalSeconds;
                GeneralHelper.WriteToConsole(Properties.Resources.FailedGOE, timePassed);
                GeneralHelper.WriteToConsole(t.Exception.Message);
                foreach(var ex in t.Exception.InnerExceptions)
                {
                    GeneralHelper.WriteToConsole(ex.ToString());
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

            rootTemplateTask.ContinueWith(delegate {
                if(Loaded)
                {
                    if(GameObjects.Count == 0)
                    {
                        GeneralHelper.WriteToConsole(Properties.Resources.NoGameObjectsFound);
                        System.Windows.Application.Current.Dispatcher.Invoke(() => {
                            GameObjectViewModel.View.Close();
                        });
                    }
                    else
                    {
                        var timePassed = DateTime.Now.Subtract(start).TotalSeconds;
                        GeneralHelper.WriteToConsole(Properties.Resources.LoadedGOE, timePassed);
                        GameObjectViewModel.Loaded = true;
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
            GameObjects?.Clear();
            TranslationLookup?.Clear();
            Translations?.Clear();
            Races?.Clear();
            StatStructures?.Clear();
            TextureAtlases?.Clear();
            PakReaderHelpers.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        public async Task<string> LoadRootTemplates()
        {
            return await Task.Run(() => {
                GameObjectTypes = Enum.GetValues(typeof(GameObjectType)).Cast<GameObjectType>().OrderBy(got => got).ToList();
                PakReaderHelpers = PakReaderHelper.GetPakHelpers();

                ReadVisualBanks();
                CloseFileProgress();
                if (GameObjectViewModel.LoadingCanceled) goto canceled;
                ReadTranslations();
                CloseFileProgress();
                if (GameObjectViewModel.LoadingCanceled) goto canceled;
                //ReadTextureBanks();
                ReadRootTemplate();
                CloseFileProgress();
                if (GameObjectViewModel.LoadingCanceled) goto canceled;
                ReadData();
                if (GameObjectViewModel.LoadingCanceled) goto canceled;
                ReadIcons();
                if (GameObjectViewModel.LoadingCanceled) goto canceled;
                SortRootTemplate();
                if (GameObjectViewModel.LoadingCanceled) goto canceled;

                canceled:
                if (GameObjectViewModel.LoadingCanceled) {
                    return null;
                }

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

            var selectedLanguage = ViewModels.MainWindow.GetSelectedLanguage();
            var selectedLanguagePath = selectedLanguage.LocaPath;
            var translationFile = FileHelper.GetPath(selectedLanguagePath);

            var xmlPath = Path.ChangeExtension(selectedLanguagePath, ".xml");
            var translationFileConverted = FileHelper.GetPath(xmlPath);
            var pathCheck = translationFileConverted.Replace(".xml", ".loca.xml");
            if (!File.Exists(translationFileConverted) && File.Exists(pathCheck))
            {
                translationFileConverted = pathCheck;
            } 
            else if(!File.Exists(translationFileConverted) && !File.Exists(pathCheck))
            {
                var helper = PakReaderHelpers.First(h => h.PakName == selectedLanguagePath.Split('\\')[0]);
                helper.DecompressPakFile(PakReaderHelper.GetPakPath(selectedLanguagePath));
                translationFileConverted = pathCheck;
            }

            if (!File.Exists(translationFileConverted) && File.Exists(translationFile))
            {
                using (var fs = File.Open(translationFile, System.IO.FileMode.Open))
                {
                    var resource = LocaUtils.Load(fs, LocaFormat.Loca);
                    LocaUtils.Save(resource, pathCheck, LocaFormat.Xml);
                    translationFileConverted = pathCheck;
                }
            }

            if (File.Exists(translationFileConverted))
            {
                if (!FileHelper.TryParseXml(translationFileConverted))
                {
                    translationFileConverted = translationFileConverted.Replace($"{FileHelper.UnpackedDataPath}\\", string.Empty);
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
            var locaPathSplit = selectedLanguage.LocaPath.Split('\\');
            GeneralHelper.WriteToConsole(Properties.Resources.FailedToFindTranslationPak, locaPathSplit.Last().Split('.').First()+".xml", locaPathSplit.First());
            return false;
        }

        /// <summary>
        /// Reads the root template and converts it into a GameObject list.
        /// </summary>
        /// <returns>Whether the root template was read.</returns>
        private bool ReadRootTemplate()
        {
            var deserializedGameObjects = FileHelper.DeserializeObject<List<GameObject>>("GameObjects");
            if (deserializedGameObjects != null)
            {
                GameObjects = deserializedGameObjects;
                GameObjectsCached = true;
                return true;
            }

            GeneralHelper.WriteToConsole(Properties.Resources.ReadingGameObjects);
            var rootTemplates = GetFileList("GameObjects");
            ShowFileProgress(rootTemplates.Count);
#if DEBUG
            var typeBag = new ConcurrentBag<string>();
            var idBag = new ConcurrentBag<string>();
            var classBag = new ConcurrentBag<Tuple<string, string>>();
#endif
            Parallel.ForEach(rootTemplates, GeneralHelper.ParallelOptions, (rootTemplate, loopState) =>
            {
                if (GameObjectViewModel.LoadingCanceled) loopState.Break();
                var fileToExist = rootTemplate;
                rootTemplate = FileHelper.GetPath(rootTemplate);

                var fileExists = File.Exists(rootTemplate) || File.Exists(rootTemplate + ".lsx");
                if (!fileExists)
                {
                    var helper = PakReaderHelpers.First(h => h.PakName == fileToExist.Split('\\')[0]);
                    helper.DecompressPakFile(PakReaderHelper.GetPakPath(fileToExist));
                    fileExists = true;
                }

                if (fileExists)
                {
                    var rootTemplatePath = FileHelper.Convert(rootTemplate, "lsx", Path.ChangeExtension(rootTemplate, "lsx"));
                    if (File.Exists(FileHelper.GetPath(rootTemplatePath)))
                    {
                        try
                        {
                            var pak = Regex.Match(rootTemplatePath, @"(?<=UnpackedData\\).*?(?=\\)").Value;
                            using (XmlReader reader = XmlReader.Create(FileHelper.GetPath(rootTemplatePath)))
                            {
                                reader.MoveToContent();
                                while (!reader.EOF)
                                {
                                    if (GameObjectViewModel.LoadingCanceled) loopState.Break();
                                    if (reader.Name == "region")
                                    {
                                        if (!reader.ReadToDescendant("children"))
                                        {
                                            reader.ReadToFollowing("region");
                                        }

                                        reader.ReadToDescendant("node");
                                        do
                                        {
                                            var gameObject = new GameObject { Pak = pak, Children = new List<GameObject>(), FileLocation = fileToExist };
                                            reader.ReadToDescendant("attribute");
                                            do
                                            {
                                                var id = reader.GetAttribute("id");
                                                var handle = reader.GetAttribute("handle");
                                                var value = handle ?? reader.GetAttribute("value");
                                                var type = reader.GetAttribute("type");
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
                                                    if (value != null && TranslationLookup.ContainsKey(value))
                                                    {
                                                        var translationText = TranslationLookup[value].Value;
                                                        gameObject.LoadProperty(id, type, translationText);
                                                    }
                                                }
                                            } while (reader.ReadToNextSibling("attribute"));
                                            if (string.IsNullOrEmpty(gameObject.ParentTemplateId))
                                                gameObject.ParentTemplateId = gameObject.TemplateName;
                                            if (string.IsNullOrEmpty(gameObject.Name))
                                                gameObject.Name = gameObject.DisplayName;
                                            if (string.IsNullOrEmpty(gameObject.Name))
                                                gameObject.Name = gameObject.Stats;

                                            GameObjectBag.Add(gameObject);
                                        } while (reader.ReadToNextSibling("node"));
                                    }
                                    reader.ReadToFollowing("region");
                                }
                            }
                        }
                        catch
                        {
                            rootTemplate = rootTemplate.Replace($"{FileHelper.UnpackedDataPath}\\", string.Empty);
                            GeneralHelper.WriteToConsole(Properties.Resources.CorruptXmlFile, rootTemplate);
                            return;
                        }
                    }
                    UpdateFileProgress();
                }
            });
            CloseFileProgress();
            if (GameObjectViewModel.LoadingCanceled) return false;
            #if DEBUG
            FileHelper.SerializeObject(typeBag.ToList().Distinct().ToList(), "GameObjectTypes");
            FileHelper.SerializeObject(idBag.ToList().Distinct().ToList(), "GameObjectAttributeIds");
            GeneralHelper.ClassBuilder(classBag.ToList().Distinct().ToList());
            #endif

            if(GameObjectBag.Count == 0)
            {
                return false;
            }
            if (GameObjectViewModel.LoadingCanceled) return false;
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
            var degree = GeneralHelper.ParallelOptions.MaxDegreeOfParallelism;
            degree = degree < 1 ? 4 : degree;
            var orderedChildren = children.AsParallel().WithDegreeOfParallelism(degree).OrderBy(go => string.IsNullOrEmpty(go.Name)).ThenBy(go => go.Name);
            var lookup = GameObjects.Where(go => !string.IsNullOrEmpty(go.MapKey)).GroupBy(go => go.MapKey).ToDictionary(go => go.Key, go => go.Last());
            Parallel.ForEach(orderedChildren, GeneralHelper.ParallelOptions, (gameObject, loopState) =>
            {
                if (GameObjectViewModel.LoadingCanceled) loopState.Break();
                if (lookup.ContainsKey(gameObject.ParentTemplateId))
                {
                    var goChildren = lookup[gameObject.ParentTemplateId].Children;
                    if (goChildren != null)
                    {
                        lock (goChildren)
                            goChildren.Add(gameObject);
                    }
                }
            });
            if (GameObjectViewModel.LoadingCanceled) return false;
            GameObjects = GameObjects.Where(go => string.IsNullOrEmpty(go.ParentTemplateId)).ToList();
            foreach(var gameObject in GameObjects)
            {
                if (GameObjectViewModel.LoadingCanceled) return false;
                gameObject.PassOnStats();
            }
            if (GameObjectViewModel.LoadingCanceled) return false;
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
        /// <returns>Whether the stats were read.</returns>
        private bool ReadData()
        {
            foreach(var pak in PakReaderHelpers)
            {
                var files = pak.PackagedFiles.Where(pf => pf.Name.Contains("Stats/Generated/Data"))
                    .Where(pf => Path.GetExtension(pf.Name) == ".txt" && !ExcludedData.Contains(Path.GetFileNameWithoutExtension(pf.Name)))
                    .Select(pf => pf.Name);

                foreach (var file in files)
                {
                    if (GameObjectViewModel.LoadingCanceled) return false;
                    var fileType = Models.StatStructures.StatStructure.FileType(file);
                    var line = string.Empty;

                    var contents = pak.ReadPakFileContents(file);

                    using (var ms = new System.IO.MemoryStream(contents))
                    using (var fileStream = new System.IO.StreamReader(ms))
                    {
                        while ((line = fileStream.ReadLine()) != null)
                        {
                            if (line.Contains("new entry"))
                            {
                                StatStructures.Add(Models.StatStructures.StatStructure.New(fileType, line.Substring(10)));
                            }
                            else if (line.IndexOf("type") == 0)
                            {
                                if (StatStructures.Count != 0)
                                    StatStructures.Last().Type = fileType;
                            }
                            else if (line.IndexOf("using") == 0)
                            {
                                if (StatStructures.Count != 0)
                                    StatStructures.Last().InheritProperties(line, StatStructures);
                            }
                            else if (!string.IsNullOrEmpty(line))
                            {
                                if(StatStructures.Count != 0)
                                    StatStructures.Last().LoadProperty(line);
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Reads the texture atlas for icon displays.
        /// </summary>
        /// <returns>Whether the texture atlas was created.</returns>
        private bool ReadIcons()
        {
            var iconFiles = GetFileList("IconUVList");
            foreach(var iconFile in iconFiles)
            {
                var helper = PakReaderHelpers.FirstOrDefault(h => h.PakName == iconFile.Split('\\')[0]);
                if (helper != null)
                {
                    var contents = helper.ReadPakFileContents(PakReaderHelper.GetPakPath(iconFile));
                    TextureAtlases.Add(TextureHelper.Read(contents, iconFile, new DirectoryInfo(iconFile).Parent.Parent.Name));
                }
            }
            return true;
        }

        /// <summary>
        /// Reads the visual banks for a list of id/filepath references for quick lookup.
        /// </summary>
        /// <returns>Whether the visual bank lists were created.</returns>
        private bool ReadVisualBanks()
        {
            #region Setup
            var deserializedCharacterVisualBanks = FileHelper.DeserializeObject<Dictionary<string, string>>("CharacterVisualBanks");
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

            if (GameObjectViewModel.LoadingCanceled) return false;

            GeneralHelper.WriteToConsole(Resources.LoadingBankFiles);

            // Lookup CharacterVisualBank file from CharacterVisualResourceID
            var characterVisualBanks = new ConcurrentDictionary<string, string>();
            var visualBanks = new ConcurrentDictionary<string, string>();
            var bodySetVisuals = new ConcurrentDictionary<string, string>();
            var materialBanks = new ConcurrentDictionary<string, string>();
            var textureBanks = new ConcurrentDictionary<string, string>();
            var characterVisualBanksFiles = GetFileList("CharacterVisualBank");
            if (characterVisualBanksFiles.Count > 0)
                GeneralHelper.WriteToConsole(Resources.FoundCharacterVisualBanks);
            if (GameObjectViewModel.LoadingCanceled) return false;
            var visualBankFiles = GetFileList("VisualBank");
            if (visualBankFiles.Count > 0)
                GeneralHelper.WriteToConsole(Resources.FoundVisualBanks);
            if (GameObjectViewModel.LoadingCanceled) return false;
            var materialBankFiles = GetFileList("MaterialBank");
            if (GameObjectViewModel.LoadingCanceled) return false;
            if (materialBankFiles.Count > 0)
                GeneralHelper.WriteToConsole(Resources.FoundMaterialBanks);
            var textureBankFiles = GetFileList("TextureBank");
            if (GameObjectViewModel.LoadingCanceled) return false;
            if (textureBankFiles.Count > 0)
                GeneralHelper.WriteToConsole(Resources.FoundTextureBanks);
            visualBankFiles.AddRange(materialBankFiles);
            visualBankFiles.AddRange(textureBankFiles);
            visualBankFiles.AddRange(characterVisualBanksFiles);
            visualBankFiles = visualBankFiles.Distinct().ToList();
            if (visualBankFiles.Count > 0)
            {
                ShowFileProgress(visualBankFiles.Count);
                GeneralHelper.WriteToConsole(Resources.SortingBanksFiles);
            }

            #endregion
            Parallel.ForEach(visualBankFiles, GeneralHelper.ParallelOptions, (visualBankFile, loopTask) =>
            {
                if (GameObjectViewModel.LoadingCanceled) loopTask.Break();
                var fileToExist = FileHelper.GetPath(visualBankFile);
                var fileExists = File.Exists(fileToExist) || File.Exists(fileToExist + ".lsx");
                if(!fileExists)
                {
                    var helper = PakReaderHelpers.First(h => h.PakName == visualBankFile.Split('\\')[0]);
                    helper.DecompressPakFile(PakReaderHelper.GetPakPath(visualBankFile));
                    fileExists = true;
                }

                if (fileExists)
                {
                    XmlReader reader = null;

                    var visualBankFilePath = FileHelper.Convert(visualBankFile, "lsx", Path.ChangeExtension(visualBankFile, "lsx"));

                    try
                    {
                        using (var ms = new System.IO.StreamReader("\\\\?\\"+FileHelper.GetPath(visualBankFilePath)))
                        {
                            reader = XmlReader.Create(ms);
                            reader.MoveToContent();
                            while (!reader.EOF)
                            {
                                if (GameObjectViewModel.LoadingCanceled) loopTask.Break();
                                if (reader.Name == "region")
                                {
                                    var sectionId = reader.GetAttribute("id");
                                    var isCharacterVisualBank = sectionId == "CharacterVisualBank";
                                    var isVisualBank = sectionId == "VisualBank";
                                    var isMaterialBank = sectionId == "MaterialBank";
                                    var isTextureBank = sectionId == "TextureBank";

                                    if (isCharacterVisualBank || isVisualBank || isMaterialBank || isTextureBank)
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
                                            var bodySetVisual = string.Empty;
                                            do
                                            {
                                                var attributeId = reader.GetAttribute("id");
                                                if (attributeId == "ID")
                                                {
                                                    resourceId = reader.GetAttribute("value");
                                                }
                                                else if (attributeId == "BodySetVisual")
                                                {
                                                    bodySetVisual = reader.GetAttribute("value");
                                                    if (!string.IsNullOrEmpty(bodySetVisual))
                                                        bodySetVisuals.TryAdd(bodySetVisual, visualBankFile + ".lsx");
                                                }
                                            } while (reader.ReadToNextSibling("attribute"));

                                            if (isCharacterVisualBank)
                                                characterVisualBanks.TryAdd(resourceId, visualBankFile + ".lsx");
                                            else if (isMaterialBank)
                                                materialBanks.TryAdd(resourceId, visualBankFile + ".lsx");
                                            else if (isTextureBank)
                                                textureBanks.TryAdd(resourceId, visualBankFile + ".lsx");
                                            else if (isVisualBank)
                                                visualBanks.TryAdd(resourceId, visualBankFile + ".lsx");
                                        } while (reader.ReadToNextSibling("node"));
                                    }
                                }
                                reader.ReadToFollowing("region");
                            }
                        }
                    }
                    catch
                    {
                        GeneralHelper.WriteToConsole(Resources.CorruptXmlFile, visualBankFile);
                        if(reader != null)
                        {
                            reader.Dispose();
                        }
                        GameObjectViewModel.LoadingCanceled = true;
                    }
                    finally
                    {
                        if (reader != null)
                        {
                            reader.Dispose();
                        }
                        UpdateFileProgress();
                    }
                }
            });
            CloseFileProgress();
            if (GameObjectViewModel.LoadingCanceled) return false;
            CharacterVisualBanks = characterVisualBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            VisualBanks = visualBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            BodySetVisuals = bodySetVisuals.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            MaterialBanks = materialBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            TextureBanks = textureBanks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            if (GameObjectViewModel.LoadingCanceled) return false;
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

            // Use fast query first in case user has decompressed all files
            IndexHelper.SearchFiles($"id=\"{searchTerm}\"", false, null, false).ContinueWith(results => {
                rtList.AddRange(results.Result.Matches.Where(r => r.EndsWith(".lsx") || r.EndsWith(".lsf")));
            }).Wait();

            // Double check that files exist in index
            if(rtList.Count == 0)
            {
                IndexHelper.SearchFiles(searchTerm, false, null, false).ContinueWith(results => {
                    rtList.AddRange(results.Result.Matches.Where(r => r.EndsWith(".lsf")));
                }).Wait();
            }
            return rtList;
        }

        /// <summary>
        /// Deletes the GameObject cache, if it exists
        /// </summary>
        public static void ClearGameObjectCache()
        {
            var cacheDirectory = "Cache";
            try
            {
                if (Directory.Exists(cacheDirectory))
                {
                    // check if cache exists
                    var result = System.Windows.Forms.MessageBox.Show(Properties.Resources.GOEDeleteQuestion, 
                        Properties.Resources.GOEClearCacheButton, System.Windows.Forms.MessageBoxButtons.OKCancel);

                    if (result.Equals(System.Windows.Forms.DialogResult.OK))
                    {
                        Directory.Delete(cacheDirectory, true);
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

        /// <summary>
        /// Displays the file progress bar and initilizes the data fields
        /// Disables index and file buttons
        /// </summary>
        /// <param name="fileCount">The number of files</param>
        private static void ShowFileProgress(int fileCount)
        {
            App.Current.Dispatcher.Invoke(() => {
                var dataContext = (App.Current.MainWindow.DataContext as ViewModels.MainWindow);
                if (dataContext != null)
                {
                    dataContext.SearchResults.IsIndexing = true;
                    dataContext.SearchResults.IndexFileTotal = fileCount;
                    dataContext.SearchResults.IndexStartTime = DateTime.Now;
                    dataContext.SearchResults.IndexFileCount = 0;
                }
            });
        }

        /// <summary>
        /// Updates the file progress bar
        /// </summary>
        private static void UpdateFileProgress()
        {
            App.Current.Dispatcher.Invoke(() => {
                var dataContext = (App.Current.MainWindow.DataContext as ViewModels.MainWindow);
                if (dataContext != null)
                    dataContext.SearchResults.IndexFileCount++;
            });
        }

        /// <summary>
        /// Hides the file progress bar and re-enables the buttons
        /// </summary>
        private static void CloseFileProgress()
        {
            App.Current.Dispatcher.Invoke(() => {
                var dataContext = (App.Current.MainWindow.DataContext as ViewModels.MainWindow);
                if (dataContext != null)
                    dataContext.SearchResults.IsIndexing = false;
            });
        }
    }
}
