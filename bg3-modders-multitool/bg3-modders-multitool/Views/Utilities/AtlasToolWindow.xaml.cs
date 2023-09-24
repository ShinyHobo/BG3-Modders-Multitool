namespace bg3_modders_multitool.Views.Utilities
{
    using bg3_modders_multitool.ViewModels.Utilities;
    using System.Windows;

    /// <summary>
    /// Interaction logic for AtlasToolWindow.xaml
    /// </summary>
    public partial class AtlasToolWindow : Window
    {
        private AtlasToolViewModel _viewModel;
        public AtlasToolWindow()
        {
            InitializeComponent();
            _viewModel = new AtlasToolViewModel();
            DataContext = _viewModel;
        }

        #region Atlas to Frames
        /// <summary>
        /// Select the file to pull frames from
        /// </summary>
        private void fileSelectS2F_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectSheetInput();
        }

        /// <summary>
        /// Select the folder to place frames
        /// </summary>
        private void outputFolderSelectS2F_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectFramesOutput();
        }

        /// <summary>
        /// Pull frames from file
        /// </summary>
        private void convertToFrames_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ConvertAtlasToFrames();
        }

        #endregion

        #region Frames to Atlas
        /// <summary>
        /// Select the files to convert to a sheet
        /// </summary>
        private void fileSelectF2S_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectFramesInput();
        }

        /// <summary>
        /// Select where to save the sheet
        /// </summary>
        private void outputFolderSelectF2S_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAtlasOutput();
        }

        /// <summary>
        /// Convert the selected frames into a sheet
        /// </summary>
        private void convertToSheet_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ConvertFramesToAtlas();
        }
        #endregion
    }
}
