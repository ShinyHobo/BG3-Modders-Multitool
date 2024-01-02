/// <summary>
/// The meta lsx validation window viewmodel.
/// </summary>
namespace bg3_modders_multitool.ViewModels.Utilities
{
    using bg3_modders_multitool.Services;
    using System;
    using System.Linq;
    using System.Xml.Linq;

    public class AddMissingMetaLsxViewModel : BaseViewModel
    {
        public AddMissingMetaLsxViewModel(string modPath)
        {
            ModPath = modPath;
            ModName = modPath.Split('\\').Last();
            WindowTitle = string.Format(Properties.Resources.AddMissingMetaLsxTitle, ModName);
        }

        public string MetaPath { get; private set; }
        private string ModPath { get; set; }
        private string ModName { get; set; }

        private string _windowTitle;
        public string WindowTitle
        {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Validates user input and generates a meta.lsx file in the correct directory
        /// </summary>
        /// <param name="author">The author name</param>
        /// <param name="description">The mod description</param>
        public void GenerateMetaLsx(string author, string description, LSLib.LS.PackedVersion version)
        {
            if(!string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(description))
            {
                var xmlText = FileHelper.LoadFileTemplate("meta.lsx");
                var xml = XDocument.Parse(xmlText);
                xml.Descendants("attribute").Where(n => n.Attribute("id").Value == "Author").Single().Attribute("value").Value = author;
                xml.Descendants("attribute").Where(n => n.Attribute("id").Value == "Description").Single().Attribute("value").Value = description;
                xml.Descendants("attribute").Where(n => n.Attribute("id").Value == "Folder").Single().Attribute("value").Value = ModName;
                xml.Descendants("attribute").Where(n => n.Attribute("id").Value == "Name").Single().Attribute("value").Value = ModName;
                xml.Descendants("attribute").Where(n => n.Attribute("id").Value == "UUID").Single().Attribute("value").Value = Guid.NewGuid().ToString();
                xml.Descendants("attribute").Where(n => n.Attribute("id").Value == "Version64").ToList().ForEach(n => { n.Attribute("value").Value = version.ToVersion64().ToString(); });

                MetaPath = $"{ModPath}\\meta.lsx";
                xml.Save(MetaPath);
            }
        }
    }
}
