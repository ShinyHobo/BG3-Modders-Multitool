namespace bg3_modders_multitool.Views.Utilities
{
    using bg3_modders_multitool.ViewModels.Utilities;
    using System.Windows;

    /// <summary>
    /// Interaction logic for AtlasToolWindow.xaml
    /// </summary>
    public partial class AtlasToolWindow : Window
    {
        public AtlasToolWindow()
        {
            InitializeComponent();
            DataContext = new AtlasToolViewModel();
        }

        #region Atlas to Frames
        /// <summary>
        /// Select the file to pull frames from
        /// </summary>
        private void fileSelectS2F_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Select the folder to place frames
        /// </summary>
        private void outputFolderSelectF2S_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Pull frames from file
        /// </summary>
        private void convertToFrames_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Select how many images wide the sheet is
        /// </summary>
        private void horizontalFramesInSheet_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        /// <summary>
        /// Select how many images tall the sheet is
        /// </summary>
        private void verticalFramesInSheet_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        #endregion

        #region Frames to Atlas
        /// <summary>
        /// Select the files to convert to a sheet
        /// </summary>
        private void fileSelectF2S_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Select the folder to place sheet
        /// </summary>
        private void outputFolderSelectS2F_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Select how many files wide the sheet should be
        /// </summary>
        private void horizontalFramesForSheet_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

        }

        /// <summary>
        /// Convert the selected frames into a sheet
        /// </summary>
        private void convertToSheet_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }
}
