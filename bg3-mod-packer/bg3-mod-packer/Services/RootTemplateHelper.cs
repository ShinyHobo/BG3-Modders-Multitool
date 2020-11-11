namespace bg3_mod_packer.Services
{
    using bg3_mod_packer.Models;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;

    public class RootTemplateHelper
    {
        private static string[] GameObjectTypes = { "character","item","scenery","prefab","trigger","surface","projectile","decal","TileConstruction","light","LevelTemplate","SplineConstruction","lightProbe","Spline","terrain" };

        private List<GameObject> gameObjects;

        public RootTemplateHelper()
        {
            ReadRootTemplate();
        }

        /// <summary>
        /// Reads the root template and converts it into a GameObject list.
        /// </summary>
        /// <returns>The time to complete.</returns>
        private double ReadRootTemplate()
        {
            var start = DateTime.Now;
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
                                    gameObject = new GameObject();
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
                                            gameObject.DisplayName = value;
                                            break;
                                        case "Description":
                                            gameObject.Description = value;
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
                var temp = gameObjects.Where(go => string.IsNullOrEmpty(go.ParentTemplateId)).ToList();

                foreach(var gameObject in temp)
                {
                    gameObject.Children = GetChildren(gameObject);
                }

                gameObjects = temp;

                return DateTime.Now.Subtract(start).TotalSeconds;
            }
            return -1;
        }

        public async Task<List<GameObject>> LoadRelevent(string gameObjectType)
        {
            return await Task.Run(() => {
                var start = DateTime.Now;
                CheckForValidGameObjectType(gameObjectType);
                var returnObjects = gameObjects.Where(go => go.Type == gameObjectType).ToList();
                var timePassed = DateTime.Now.Subtract(start).TotalSeconds;
                return returnObjects;
            });
        }

        #region Private Methods
        /// <summary> 
        /// Generates game objects for each child of the given game object.
        /// </summary>
        /// <param name="gameObject">The parent game object.</param>
        /// <returns>The list of child game objects.</returns>
        private List<GameObject> GetChildren(GameObject gameObject)
        {
            var matchingGameObjects = gameObjects.Where(go => go.ParentTemplateId == gameObject.MapKey).ToList();
            foreach (var matchingGameObject in matchingGameObjects)
            {
                matchingGameObject.Children = GetChildren(matchingGameObject);
            }
            return matchingGameObjects;
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
