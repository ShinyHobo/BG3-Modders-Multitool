/// <summary>
/// The game object model.
/// </summary>
namespace bg3_modders_multitool.Models
{
    using bg3_modders_multitool.Enums;
    using bg3_modders_multitool.Models.GameObjectTypes;
    using bg3_modders_multitool.Properties;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class GameObject
    {
        #region Parameters
        public string Pak { get; set; }
        public List<GameObject> Children { get; set; }
        public GameObjectType Type { get; set; }
        public string FileLocation { get; set; }
        public string MapKey { get; set; }
        public string ParentTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string Name { get; set; }
        public string DisplayNameHandle { get; set; }
        public string DisplayName { get; set; }
        public string DescriptionHandle { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Stats { get; set; }
        public string CharacterVisualResourceID { get; set; }
        public string VisualTemplate { get; set; }
        public string TitleHandle { get; set; }
        public string Title { get; set; }

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
                if (string.IsNullOrEmpty(go.Stats))
                {
                    go.Stats = Stats;
                }
                if (string.IsNullOrEmpty(go.Icon))
                {
                    go.Icon = Icon;
                }
                if (string.IsNullOrEmpty(go.CharacterVisualResourceID))
                {
                    go.CharacterVisualResourceID = CharacterVisualResourceID;
                }
                if (string.IsNullOrEmpty(go.VisualTemplate))
                {
                    go.VisualTemplate = VisualTemplate;
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
            return Name?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   Pak.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   MapKey?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   ParentTemplateId?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   DisplayNameHandle?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   DisplayName?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   DescriptionHandle?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   Description?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   Icon?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   Stats?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
        #endregion

        /// <summary>
        /// Loads gameobject properties using reflection.
        /// </summary>
        /// <param name="id">The attribute id.</param>
        /// <param name="type">The attribute type.</param>
        /// <param name="value">The attribute value.</param>
        public void LoadProperty(string id, string type, string value)
        {
            if(type != null)
            {
                if (type.Contains("String"))
                {
                    var property = GetType().GetProperty(id);
                    if (property != null)
                    {
                        try
                        {
                            var propertyType = property.PropertyType;
                            if (propertyType.IsEnum)
                            {
                                property.SetValue(this, Enum.Parse(property.PropertyType, value), null);
                            }
                            else
                            {
                                if (propertyType == typeof(string))
                                {
                                    property.SetValue(this, value);
                                }
                                else
                                {
                                    property.SetValue(this, new StringType(value, type));
                                }
                            }
                        } 
                        catch
                        {
                            // This can usually be fixed by adding properties to the given property type
                            #if DEBUG
                            Services.GeneralHelper.WriteToConsole(Resources.ErrorParsingProperty, value, property.PropertyType.Name);
                            #endif
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts a given xml attribute to the corresponding C# valuetype.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns>The property.</returns>
        public static object XmlToValue(string type, string value)
        {
            object propertyValue = null;
            switch (type)
            {
                case "LSString":
                    propertyValue = new LSString(value);
                    break;
                case "FixedString":
                    propertyValue = new FixedString(value);
                    break;
                case "TranslatedString":
                    propertyValue = new TranslatedString(value);
                    break;
                case "int8":
                    propertyValue = sbyte.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "int16":
                    propertyValue = short.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "int":
                case "int32":
                    propertyValue = int.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "uint8":
                    propertyValue = byte.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "uint16":
                    propertyValue = ushort.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "uint32":
                    propertyValue = uint.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "uint64":
                    propertyValue = ulong.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "bool":
                    propertyValue = bool.Parse(value);
                    break;
                case "guid":
                    propertyValue = new Guid(value);
                    break;
                case "float":
                    propertyValue = float.Parse(value, CultureInfo.InvariantCulture);
                    break;
                case "fvec2":
                    var fvec2 = value.Split(' ').Select(v => float.Parse(v, CultureInfo.InvariantCulture)).ToArray();
                    propertyValue = new Tuple<float, float>(fvec2[0], fvec2[1]);
                    break;
                case "fvec3":
                    var fvec3 = value.Split(' ').Select(v => float.Parse(v, CultureInfo.InvariantCulture)).ToArray();
                    propertyValue = new Tuple<float, float, float>(fvec3[0], fvec3[1], fvec3[2]);
                    break;
                case "fvec4":
                    var fvec4 = value.Split(' ').Select(v => float.Parse(v, CultureInfo.InvariantCulture)).ToArray();
                    propertyValue = new Tuple<float, float, float, float>(fvec4[0], fvec4[1], fvec4[2], fvec4[3]);
                    break;
                case "double":
                    propertyValue = double.Parse(value, CultureInfo.InvariantCulture);
                    break;
                default:
                    Services.GeneralHelper.WriteToConsole(Resources.GameObjectUncovered, type);
                    break;
            }
            return propertyValue;
        }
    }
}
