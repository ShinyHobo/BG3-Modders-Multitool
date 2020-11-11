namespace bg3_mod_packer.Services
{
    using bg3_mod_packer.Models;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public class RootTemplateHelper
    {
        private static string[] GameObjectTypes = { "character","item","scenery","prefab","trigger","surface","projectile","decal","TileConstruction","light","LevelTemplate","SplineConstruction","lightProbe","Spline","terrain" };

        private IEnumerable<XElement> gameObjects;

        public async Task<List<GameObject>> LoadRelevent(string gameObjectType)
        {
            return await Task.Run(() => {
                var start = DateTime.Now;
                CheckForValidGameObjectType(gameObjectType);
                var rootTemplates = @"Shared\Public\Shared\RootTemplates\_merged.lsf";
                if (File.Exists(FileHelper.GetPath(rootTemplates)))
                {
                    rootTemplates = FileHelper.GetPath(FileHelper.ConvertToLsx(rootTemplates));
                    XDocument doc = XDocument.Load(rootTemplates);
                    gameObjects = doc.Descendants("node").Where(node => node.Attribute("id").Value == "GameObjects" &&
                        node.Elements("attribute").FirstOrDefault(n => n.Attribute("id").Value == "Type" && n.Attribute("value").Value == gameObjectType) != null);
                    var toplevelGameObjects = gameObjects.Where(go => go.Elements("attribute").FirstOrDefault(a => a.Attribute("id").Value == "ParentTemplateId" && !string.IsNullOrEmpty(a.Attribute("value").Value)) == null);
                    var gameObjectList = GenerateGameObjects(toplevelGameObjects);
                    var timePassed = DateTime.Now.Subtract(start).TotalSeconds;
                    return gameObjectList;
                }
                return null;
            });
        }

        #region Private Methods
        /// <summary>
        /// Generates game objects for each child of the given parent template.
        /// </summary>
        /// <param name="parentTemplateId">The parent template UUID.</param>
        /// <returns>The list of child game objects.</returns>
        private List<GameObject> GetChildren(string parentTemplateId)
        {
            var nodes = gameObjects.Where(go => go.Elements("attribute").FirstOrDefault(a => a.Attribute("id").Value == "ParentTemplateId" && a.Attribute("value").Value == parentTemplateId) != null);
            return GenerateGameObjects(nodes);
        }

        /// <summary>
        /// Generates a list of game objects.
        /// </summary>
        /// <param name="nodes">The nodes to generate game objects for.</param>
        /// <returns>The generated game object list.</returns>
        private List<GameObject> GenerateGameObjects(IEnumerable<XElement> nodes)
        {
            var gameObjectList = new ConcurrentBag<GameObject>();
            Parallel.ForEach(nodes, node =>
            {
                var gameObject = GenerateGameObject(node);
                gameObject.Children = GetChildren(gameObject.MapKey);
                gameObjectList.Add(gameObject);
            });
            return gameObjectList.OrderBy(go => go.Name).ToList();
        }

        /// <summary>
        /// Generates a game object for an xml node.
        /// </summary>
        /// <param name="node">The node to generate a game object for.</param>
        /// <returns>The generated game object.</returns>
        private GameObject GenerateGameObject(XElement node)
        {
            var attributes = node.Elements("attribute");
            return new GameObject
            {
                MapKey = attributes.Where(a => a.Attribute("id").Value == "MapKey").Select(a => a.Attribute("value").Value).FirstOrDefault(),
                Name = attributes.Where(a => a.Attribute("id").Value == "Name").Select(a => a.Attribute("value").Value).FirstOrDefault(),
                Type = attributes.Where(a => a.Attribute("id").Value == "Type").Select(a => a.Attribute("value").Value).FirstOrDefault(),
                Icon = attributes.Where(a => a.Attribute("id").Value == "Icon").Select(a => a.Attribute("value").Value).FirstOrDefault(),
                Stats = attributes.Where(a => a.Attribute("id").Value == "Stats").Select(a => a.Attribute("value").Value).FirstOrDefault()
            };
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
