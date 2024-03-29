﻿/// <summary>
/// The autogenerated game object model.
/// </summary>
namespace bg3_modders_multitool.Models.GameObjects
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Services;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

	public class AutoGenGameObject
    {
        /// <summary>
        /// Finds a matching gameobject within a file and loads its model data.
        /// </summary>
        /// <param name="file">The file to search for.</param>
        /// <param name="mapKey"></param>
        public AutoGenGameObject(string file, string mapKey)
        {
			file = FileHelper.GetPath(file);
            file = FileHelper.Convert(file, "lsx", Path.ChangeExtension(file, "lsx"));
            if (File.Exists(file))
            {
                var stream = File.OpenText(file);
                using (var fileStream = stream)
                using (var reader = new XmlTextReader(fileStream))
                {
                    reader.Read();
                    var found = false;
                    while (!reader.EOF && !found)
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.IsStartElement() && reader.GetAttribute("id") == "GameObjects")
                        {
                            var xml = (XElement)XNode.ReadFrom(reader);
                            var attributes = xml.Elements().Where(x => x.Name == "attribute");
                            // Finds the matching gameobject in the file
                            if(attributes.Any(x => x.Attribute("value")?.Value == mapKey && x.Attribute("id").Value == "MapKey"))
                            {
                                found = true;
                                this.Data = new GameObjectNode(xml);
							}
                            reader.Skip();
                        }
                        else
                        {
                            reader.Read();
                        }
                    }
                    reader.Close();
                }
            }
        }

        #region Properties
        private GameObjectNode _data;

        /// <summary>
        /// The autogen model data for this gameobject.
        /// </summary>
		public GameObjectNode Data {
			get { return _data; }
			set { _data = value; }
        }
        #endregion
    }
}
