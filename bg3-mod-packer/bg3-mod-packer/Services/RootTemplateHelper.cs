namespace bg3_mod_packer.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    public static class RootTemplateHelper
    {
        private static string[] GameObjectTypes = { "character","item","scenery","prefab","trigger","surface","projectile","decal","TileConstruction","light","LevelTemplate","SplineConstruction","lightProbe","Spline","terrain" };

        public static void LoadRelevent(string gameObjectType)
        {
            CheckForValidGameObjectType(gameObjectType);
            var rootTemplates = @"Shared\Public\Shared\RootTemplates\_merged.lsf";
            if (File.Exists(FileHelper.GetPath(rootTemplates)))
            {
                rootTemplates = FileHelper.GetPath(FileHelper.ConvertToLsx(rootTemplates));
                XmlDocument doc = new XmlDocument();
                using (var fs = new FileStream(rootTemplates, FileMode.Open))
                {
                    doc.Load(fs);
                }
                var gameObjects = doc.SelectNodes("//node[@id='GameObjects']");
                var sortedGameObjects = new List<XmlNode>();
                var topLevelGameObjects = new List<XmlNode>();
                var topLevelGameObjectNames = new List<string>();
                foreach (XmlNode gameObject in gameObjects)
                {
                    var type = GetAttributeValue(gameObject, "Type");
                    CheckForValidGameObjectType(type);
                    if(type == gameObjectType)
                    {
                        sortedGameObjects.Add(gameObject);
                        var bler = GetAttributeValue(gameObject, "ParentTemplateId");
                        if(string.IsNullOrEmpty(bler))
                        {
                            topLevelGameObjects.Add(gameObject);
                            topLevelGameObjectNames.Add(GetAttributeValue(gameObject, "Name"));
                        }
                    }
                }
            }
        }

        #region Private Methods
        /// <summary>
        /// Gets the given attribute by id.
        /// </summary>
        /// <param name="node">The xml node to get the attribute of.</param>
        /// <param name="id">The xml node id.</param>
        /// <returns></returns>
        private static string GetAttributeValue(XmlNode node, string id)
        {
            return node.SelectSingleNode($"attribute[@id='{id}']").Attributes["value"].InnerText;
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
