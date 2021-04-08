/// <summary>
/// The game object model.
/// </summary>
namespace bg3_modders_multitool.Models
{
    using bg3_modders_multitool.Enums;
    using bg3_modders_multitool.Models.GameObjectTypes;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GameObject
    {
        #region Parameters
        public string Pak { get; set; }
        public List<GameObject> Children { get; set; }
        public Guid RaceUUID { get; set; }
        public GameObjectType Type { get; set; }
        public float Scale { get; set; }
        public StringType MapKey { get; set; }
        public StringType ParentTemplateId { get; set; }
        public StringType Name { get; set; }
        public StringType DisplayNameHandle { get; set; }
        public StringType DisplayName { get; set; }
        public StringType DescriptionHandle { get; set; }
        public StringType Description { get; set; }
        public StringType Icon { get; set; }
        public StringType Stats { get; set; }
        public StringType CharacterVisualResourceID { get; set; }
        public StringType LevelName { get; set; }
        public StringType TitleHandle { get; set; }
        public StringType Title { get; set; }
        public StringType PhysicsTemplate { get; set; }

        /// <summary>
        /// Gets the full depth of the tree.
        /// </summary>
        public int Depth {
            get {
                if (Children.Count == 0)
                    return 0;
                return Children.Select(x => x.Depth).DefaultIfEmpty().Max() + 1;
            }
        }

        /// <summary>
        /// Gets a count of all children in the tree
        /// </summary>
        private int count {
            get {
                if (Children == null)
                    return 0;
                return Children.Sum(x => x.count) + Children.Count;
            }
        }
        #endregion

        /// <summary>
        /// Gets a count of all children in the tree, plus the parent.
        /// </summary>
        public int Count()
        {
            return count + 1;
        }

        /// <summary>
        /// Recursive method for passing on stats to child GameObjects.
        /// </summary>
        /// <param name="gameObject">The game object to pass stats on from.</param>
        public void PassOnStats()
        {
            foreach (var go in Children)
            {
                if (string.IsNullOrEmpty(go.Stats?.Value))
                {
                    go.Stats = Stats;
                }
                if (string.IsNullOrEmpty(go.Icon?.Value))
                {
                    go.Icon = Icon;
                }
                go.PassOnStats();
            }
        }

        #region Search
        /// <summary>
        /// Recursively searches through the game object's children to find matching object names.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>The filtered game object.</returns>
        public GameObject Search(string filter)
        {
            var filteredList = new List<GameObject>();
            foreach (var subItem in Children)
            {
                var filterItem = subItem.Search(filter);
                if (filterItem != null)
                    filteredList.Add(filterItem);
            }
            var filterGo = (GameObject)MemberwiseClone();
            filterGo.Children = filteredList;
            if (filterGo.FindMatch(filter))
                return filterGo;
            else
            {
                foreach (var subItem in filterGo.Children)
                {
                    if (subItem.FindMatch(filter) || subItem.Children.Count > 0) // if children exist, it means that at least one had a match
                    {
                        return filterGo;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Finds a match to a game object property value.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        private bool FindMatch(string filter)
        {
            return Name?.Value?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   Pak.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   MapKey?.Value?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   ParentTemplateId?.Value?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   DisplayNameHandle?.Value?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   DisplayName?.Value?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   DescriptionHandle?.Value?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   Description?.Value?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   Icon?.Value?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   Stats?.Value?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
        #endregion

        public void LoadProperty(string id, string type, string value)
        {
            if(type != null)
            {
                if (type.Contains("String"))
                {
                    var property = GetType().GetProperty(id);
                    if (property != null)
                    {
                        var propertyType = property.PropertyType;
                        if (propertyType.IsEnum)
                        {
                            property.SetValue(this, Enum.Parse(property.PropertyType, value), null);
                        }
                        else
                        {
                            if(propertyType == typeof(string))
                            {
                                property.SetValue(this, value);
                            }
                            else
                            {
                                property.SetValue(this, new StringType(value, type));
                            }
                        }
                    }
                }
            }
        }
    }
}
