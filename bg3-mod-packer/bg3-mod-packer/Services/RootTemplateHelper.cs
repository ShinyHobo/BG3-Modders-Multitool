namespace bg3_mod_packer.Services
{
    using bg3_mod_packer.Models;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;

    public class RootTemplateHelper
    {
        private static string[] GameObjectTypes = { "character","item","scenery","prefab","trigger","surface","projectile","decal","TileConstruction","light","LevelTemplate","SplineConstruction","lightProbe","Spline","terrain" };

        private List<GameObject> gameObjects;
        private Dictionary<string, Translation> TranslationLookup;
        public List<GameObject> FlatGameObjects { get; private set; }
        public List<Translation> Translations { get; private set; }
        public List<Race> Races { get; private set; }

        public RootTemplateHelper()
        {
            ReadTranslations();
            ReadRootTemplate();
            ReadRaces();
        }

        private bool ReadRaces()
        {
            var raceFile = FileHelper.GetPath(@"Shared\Public\Shared\Races\Races.lsx");
            if(File.Exists(raceFile))
            {
                Races = new List<Race>();
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
                                    race = new Race();
                                }
                                var value = reader.GetAttribute("value");
                                if(reader.Depth == 5) // top level
                                {
                                    switch(id)
                                    {
                                        case "Description":
                                            race.Description = value;
                                            break;
                                        case "DisplayName":
                                            race.DisplayName = value;
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
                                if(reader.Depth == 6) // eye colors, hair colors, tags, makeup colors, skin colors, tattoo colors, visuals
                                {

                                }
                                if(reader.Depth == 7) // previous level values
                                {

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
            return false;
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
                var returnObjects = new ObservableCollection<GameObject>(gameObjects.Where(go => go.Type == gameObjectType));
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
            return false;
        }

        /// <summary>
        /// Reads the root template and converts it into a GameObject list.
        /// </summary>
        /// <returns>Whether the root template was read.</returns>
        private bool ReadRootTemplate()
        {
            var rootTemplates = @"Shared\Public\Shared\RootTemplates\_merged.lsf";
            if (File.Exists(FileHelper.GetPath(rootTemplates)))
            {
                rootTemplates = FileHelper.GetPath(FileHelper.ConvertToLsx(rootTemplates));
                gameObjects = new List<GameObject>();
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
                                    gameObject = new GameObject { Children = new List<GameObject>() };
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
                                    }
                                }
                                break;
                            case XmlNodeType.EndElement:
                                if (reader.Depth == 4) // end of GameObject
                                {
                                    gameObjects.Add(gameObject);
                                }
                                break;
                        }
                    }
                }
                gameObjects = gameObjects.OrderBy(go => go.Name).ToList();
                FlatGameObjects = gameObjects;

                // Groups children by MapKey and ParentTemplateId
                var children = gameObjects.Where(go => !string.IsNullOrEmpty(go.ParentTemplateId)).ToList();
                var lookup = gameObjects.ToDictionary(go => go.MapKey);
                foreach (var gameObject in children)
                {
                    lookup[gameObject.ParentTemplateId].Children.Add(gameObject);
                }
                gameObjects = gameObjects.Where(go => string.IsNullOrEmpty(go.ParentTemplateId)).ToList();

                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks for game object types and forces them to be accounted for.
        /// </summary>
        /// <param name="type">The game object type.</param>
        private static void CheckForValidGameObjectType(string type)
        {
            if (!GameObjectTypes.Contains(type))
            {
                throw new Exception($"RootTemplate type '{type}' not accounted for.");
            }
        }
        #endregion
    }
}
