/// <summary>
/// The drag and drop box view model.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using bg3_modders_multitool.Views.Utilities;
    using LSLib.LS;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml.Linq;

    public class DragAndDropBox : BaseViewModel
    {
        public DragAndDropBox()
        {
            PackAllowed = true;
            _packAllowedDrop = PackAllowed;
            CanRebuild = Visibility.Collapsed;
            if(Directory.Exists(Properties.Settings.Default.rebuildLocation))
            {
                LastDirectory = Properties.Settings.Default.rebuildLocation;
            }
            else
            {
                Properties.Settings.Default.rebuildLocation = null;
                Properties.Settings.Default.Save();
            }
        }

        public async Task ProcessDrop(IDataObject data)
        {
            PackAllowed = false;
            _packAllowedDrop = false;

            var differentWorkspace = false;
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                var fileDrop = data.GetData(DataFormats.FileDrop, true);
                if (fileDrop is string[] filesOrDirectories && filesOrDirectories.Length > 0)
                {
                    var workspace = filesOrDirectories[0];
                    differentWorkspace = workspace != LastDirectory;
                }
            }

            if (CanRebuild == Visibility.Collapsed || differentWorkspace)
                GetVersion();
            else
                SetVersion(data);

            await Services.DragAndDropHelper.ProcessDrop(data).ContinueWith(delegate {
                PackAllowed = true;
            });
        }

        internal void Darken()
        {
            PackBoxColor = PackAllowed ? "LightGreen" : "MidnightBlue";
            DescriptionColor = PackAllowed ? "Black" : "White";
        }

        internal void Lighten()
        {
            PackBoxColor = "LightBlue";
            DescriptionColor = "Black";
        }

        #region Version Methods
        /// <summary>
        /// Looks up the version of the first meta.lsx found in the workspace directory
        /// </summary>
        internal void GetVersion()
        {
            if (CanRebuild == Visibility.Visible)
            {
                var modsPath = Path.Combine(_lastDirectory, "Mods");

                var dir = new DirectoryInfo(modsPath);
                if(dir.Exists)
                {
                    var pathList = Directory.GetDirectories(modsPath);
                    if (pathList.Length > 0)
                    {
                        foreach (string file in Directory.GetFiles(pathList[0]))
                        {
                            if (Path.GetFileName(file).Equals("meta.lsx"))
                            {
                                var xml = FixVersion(file);
                                if (xml != null)
                                {
                                    var attributes = xml.Descendants("attribute");

                                    var version = attributes.Where(a => a.Attribute("id").Value == "Version64" && a.Parent.Attribute("id").Value == "ModuleInfo").SingleOrDefault();
                                    if (version != null)
                                    {
                                        long.TryParse(version.Attribute("value").Value, out long versionValue);
                                        var ver = PackedVersion.FromInt64(versionValue);
                                        Major = (int)ver.Major;
                                        Minor = (int)ver.Minor;
                                        Revision = (int)ver.Revision;
                                        Build = (int)ver.Build;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Properties.Settings.Default.rebuildLocation = null;
                    Properties.Settings.Default.Save();
                }
            }
        }

        /// <summary>
        /// Sets the version selected
        /// </summary>
        /// <param name="data">The file drop object data</param>
        internal void SetVersion(IDataObject data)
        {
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                var fileDrop = data.GetData(DataFormats.FileDrop, true);
                if (fileDrop is string[] filesOrDirectories && filesOrDirectories.Length > 0)
                {
                    var modsPath = Path.Combine(filesOrDirectories[0], "Mods");

                    var dir = new DirectoryInfo(modsPath);
                    if (dir.Exists)
                    {
                        var pathList = Directory.GetDirectories(modsPath);
                        foreach (string file in Directory.GetFiles(pathList[0]))
                        {
                            if (Path.GetFileName(file).Equals("meta.lsx"))
                            {
                                var xml = FixVersion(file);
                                if (xml != null)
                                {
                                    var attributes = xml.Descendants("attribute");

                                    foreach (var attribute in attributes.Where(a => a.Attribute("id").Value == "Version64"))
                                    {
                                        attribute.Attribute("value").Value = new PackedVersion()
                                        {
                                            Major = (uint)Major,
                                            Minor = (uint)Minor,
                                            Revision = (uint)Revision,
                                            Build = (uint)Build
                                        }.ToVersion64().ToString();
                                    }

                                    var version = attributes.Where(a => a.Attribute("id").Value == "Version64" && a.Parent.Attribute("id").Value == "ModuleInfo").SingleOrDefault();
                                    if (version != null)
                                    {

                                        xml.Save(file);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fixes the version from int32 to int64
        /// </summary>
        /// <param name="file">The meta.lsx file path</param>
        /// <returns>The meta.lsx xml object</returns>
        internal XDocument FixVersion(string file)
        {
            try
            {
                var xml = XDocument.Load(file);
                var attributes = xml.Descendants("attribute");
                foreach (var attribute in attributes.Where(a => a.Attribute("id").Value == "Version"))
                {
                    attribute.Attribute("id").Value = "Version64";
                    attribute.Attribute("type").Value = "int64";
                    var valid = int.TryParse(attribute.Attribute("value").Value, out int ver);
                    attribute.Attribute("value").Value = valid ? PackedVersion.FromInt32(ver).ToVersion64().ToString() : VersionCalculator.DefaultVersion.ToString();
                }
                xml.Save(file);
                return xml;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Properties
        private string _packBoxColor;

        public string PackBoxColor {
            get { return _packBoxColor; }
            set {
                _packBoxColor = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _descriptionColor;

        public string DescriptionColor {
            get { return _descriptionColor; }
            set {
                _descriptionColor = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _lastDirectory;
        public string LastDirectory {
            get { return _lastDirectory; }
            set {
                _lastDirectory = value;

                if(Directory.Exists(value) && value != Properties.Settings.Default.rebuildLocation)
                {
                    Properties.Settings.Default.rebuildLocation = value;
                    Properties.Settings.Default.Save();
                }

                CanRebuild = !string.IsNullOrEmpty(value) ? Visibility.Visible : Visibility.Collapsed;

                GetVersion();

                OnNotifyPropertyChanged();
            }
        }

        private Visibility _canRebuild;
        public Visibility CanRebuild { 
            get { return _canRebuild; } 
            set {
                _canRebuild = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _packAllowed;
        private bool _packAllowedDrop;

        public bool PackAllowed {
            get { return _packAllowed; }
            set {
                _packAllowed = value;
                if (value)
                {
                    Lighten();
                    _packAllowedDrop = value;
                }
                else
                {
                    Darken();
                }
                PackBoxInstructions = value || _packAllowedDrop ? Properties.Resources.DropModMessage : Properties.Resources.SelectDivineMessage;
                OnNotifyPropertyChanged();
            }
        }

        private string _packBoxInstructions;

        public string PackBoxInstructions {
            get { return _packBoxInstructions; }
            set {
                _packBoxInstructions = value;
                OnNotifyPropertyChanged();
            }
        }

        #region Version
        private int _major;
        public int Major { 
            get { return _major; } 
            set {
                _major = value;
                OnNotifyPropertyChanged();
            }
        }

        private int _minor;
        public int Minor
        {
            get { return _minor; }
            set
            {
                _minor = value;
                OnNotifyPropertyChanged();
            }
        }

        private int _build;
        public int Build
        {
            get { return _build; }
            set
            {
                _build = value;
                OnNotifyPropertyChanged();
            }
        }

        private int _revision;
        public int Revision
        {
            get { return _revision; }
            set
            {
                _revision = value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion
        #endregion
    }
}
