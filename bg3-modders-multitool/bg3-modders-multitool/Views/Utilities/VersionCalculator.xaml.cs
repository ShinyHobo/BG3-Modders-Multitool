namespace bg3_modders_multitool.Views.Utilities
{
    using bg3_modders_multitool.Services;
    using LSLib.LS;
    using System.Windows;

    /// <summary>
    /// Interaction logic for AddMissingMetaLsx.xaml
    /// </summary>
    public partial class VersionCalculator : Window
    {
        public static readonly ulong DefaultVersion = 36028797018963968; // 1.0.0.0

        /// <summary>
        /// Popup for adding missing meta.lsx information
        /// </summary>
        /// <param name="modPath">The mod path</param>
        public VersionCalculator()
        {
            InitializeComponent();
            int64Version.Value = DefaultVersion;
        }

        public PackedVersion PackedVersion { get; set; }
        private bool SkipValueChange { get; set; }

        public void versionSpinner_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (major != null && major.Value.HasValue &&
                minor != null &&  minor.Value.HasValue &&
                build != null && build.Value.HasValue &&
                revision != null && revision.Value.HasValue &&
                !SkipValueChange)
            {
                var packedVersion = new PackedVersion()
                {
                    Major = (uint)major.Value,
                    Minor = (uint)minor.Value,
                    Build = (uint)build.Value,
                    Revision = (uint)revision.Value
                };

                PackedVersion = packedVersion;
                int64Version.Value = (ulong?)PackedVersion.ToVersion64();
            }
        }

        private void int64Version_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(this.int64Version.Value.HasValue)
            {
                SkipValueChange = true;
                var int64Version = PackedVersion.FromInt64((long)this.int64Version.Value);
                PackedVersion = int64Version;
                major.Value = (int?)PackedVersion.Major;
                minor.Value = (int?)PackedVersion.Minor;
                build.Value = (int?)PackedVersion.Build;
                revision.Value = (int?)PackedVersion.Revision;
                SkipValueChange = false;
            }
        }

        private void CopyPatch_Click(object sender, RoutedEventArgs e)
        {
            var version = $"{PackedVersion.Major}.{PackedVersion.Minor}.{PackedVersion.Build}.{PackedVersion.Revision}";
            System.Windows.Forms.Clipboard.SetDataObject(version, false, 10, 10);
            GeneralHelper.WriteToConsole(Properties.Resources.CopiedToClipboard, version);
        }

        private void CopyInt64_Click(object sender, RoutedEventArgs e)
        {
            var version = PackedVersion.ToVersion64().ToString();
            System.Windows.Forms.Clipboard.SetDataObject(version, false, 10, 10);
            GeneralHelper.WriteToConsole(Properties.Resources.CopiedToClipboard, version);
        }
    }
}
