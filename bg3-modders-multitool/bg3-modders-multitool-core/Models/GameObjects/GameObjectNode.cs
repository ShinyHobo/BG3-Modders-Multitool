/// <summary>
/// The GameObject node model.
/// </summary>
namespace bg3_modders_multitool.Models.GameObjects
{
    using bg3_modders_multitool.Services;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class GameObjectNode
    {
        /// <summary>
        /// Creates a gameobject node, which can have a collection of attributes as well as child nodes with their own attributes.
        /// </summary>
        /// <param name="xml">The node xml to parse.</param>
        public GameObjectNode(XElement xml) 
        {
            this.Name = xml.Attribute("id").Value;
            var attributes = xml.Elements("attribute");
            if(attributes.Any())
            {
                this.Attributes = new List<GameObjectAttribute>();
                foreach (XElement attribute in attributes)
                {
                    var id = attribute.Attribute("id").Value;
                    var handle = attribute.Attribute("handle")?.Value;
                    var value = handle ?? attribute.Attribute("value").Value;
                    var type = attribute.Attribute("type").Value;
                    if (int.TryParse(type, out int typeInt))
                        type = GeneralHelper.LarianTypeEnumConvert(type);
                    this.LoadAttribute(id, type, value);
                }
            }
            
            this.Children = this.LoadChildren(xml);
        }

        /// <summary>
        /// Loads any direct children the node might have.
        /// </summary>
        /// <param name="parent">The parent xml node to parse and search for children in.</param>
        /// <returns>The list of child nodes.</returns>
        private List<GameObjectNode> LoadChildren(XElement parent)
        {
            var children = parent.Element("children");
            if (children != null)
            {
                var nodes = children.Elements("node");
                var newNodes = new List<GameObjectNode>();
                foreach (XElement node in nodes)
                {
                    newNodes.Add(new GameObjectNode(node));
                }
                return newNodes;
            }
            return null;
        }

        /// <summary>
        /// Loads a node attribute into the attributes list.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        private void LoadAttribute(string id, string type, string value)
        {
            if (char.IsLetter(id[0]) && !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(value))
            {
                var propertyValue = GameObject.XmlToValue(type, value);
                this.Attributes.Add(new GameObjectAttribute(id, propertyValue));
            }
        }

        #region Properties
        private string _name;

        /// <summary>
        /// The id of the node.
        /// </summary>
        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        private List<GameObjectAttribute> _attributes;

        /// <summary>
        /// The list of attributes for the node.
        /// </summary>
        public List<GameObjectAttribute> Attributes {
            get { return _attributes; }
            set { _attributes = value; }
        }

        private List<GameObjectNode> _children;

        /// <summary>
        /// The list of child nodes belonging to this node.
        /// </summary>
        public List<GameObjectNode> Children {
            get { return _children; }
            set { _children = value; }
        }
        #endregion
    }
}
