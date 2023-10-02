namespace bg3_modders_multitool.Views.Utilities
{
    using bg3_modders_multitool.ViewModels.Utilities;
    using LSLib.LS;
    using System.Windows;

    /// <summary>
    /// Interaction logic for AddMissingMetaLsx.xaml
    /// </summary>
    public partial class VersionCalculator : Window
    {
        /// <summary>
        /// Popup for adding missing meta.lsx information
        /// </summary>
        /// <param name="modPath">The mod path</param>
        public VersionCalculator()
        {
            InitializeComponent();
            int64Version.Value = 36028797018963968; // 1.0.0.0
        }

        private PackedVersion PackedVersion { get; set; }

        private void versionSpinner_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (major != null && major.Value.HasValue &&
                minor != null &&  minor.Value.HasValue &&
                build != null && build.Value.HasValue &&
                revision != null && revision.Value.HasValue)
            {
                var packedVersion = new PackedVersion()
                {
                    Major = (uint)major.Value,
                    Minor = (uint)minor.Value,
                    Build = (uint)build.Value,
                    Revision = (uint)revision.Value
                };

                PackedVersion = packedVersion;
                int64Version.Text = PackedVersion.ToVersion64().ToString();
            }
        }

        private void int64Version_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(this.int64Version.Value.HasValue)
            {
                var int64Version = PackedVersion.FromInt64((long)this.int64Version.Value);
                PackedVersion = int64Version;
                major.Value = (int?)PackedVersion.Major;
                minor.Value = (int?)PackedVersion.Minor;
                build.Value = (int?)PackedVersion.Build;
                revision.Value = (int?)PackedVersion.Revision;
            }
        }
    }
}
