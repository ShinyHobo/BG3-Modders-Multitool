namespace bg3_modders_multitool.ViewModels.Utilities
{
    using Alphaleonis.Win32.Filesystem;
    using Lucene.Net.Store;
    using Ookii.Dialogs.Wpf;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    public class AtlasToolViewModel : BaseViewModel
    {
        #region Atlas to Frames
        #region Properties
        private int _horizontalFramesInSheet = 1;
        /// <summary>
        /// The number of frames wide the sheet is
        /// </summary>
        public int HorizontalFramesInSheet { 
            get { return _horizontalFramesInSheet; }
            set
            {
                _horizontalFramesInSheet = value;
                OnNotifyPropertyChanged();
            }
        }

        private int _verticalFramesInSheet = 1;
        /// <summary>
        /// The number of frames tall the sheet is
        /// </summary>
        public int VerticalFramesInSheet {
            get { return _verticalFramesInSheet; }
            set
            {
                _verticalFramesInSheet = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _inputSheetFileSelection;
        /// <summary>
        /// The input sheet to pull frames from
        /// </summary>
        public string InputSheetFileSelection
        {
            get { return _inputSheetFileSelection; }
            set
            {
                _inputSheetFileSelection = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _outputFolderSelectionForFrames;
        /// <summary>
        /// The output folder to save the frames to
        /// </summary>
        public string OutputFolderSelectionForFrames
        {
            get { return _outputFolderSelectionForFrames; }
            set
            {
                _outputFolderSelectionForFrames = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// The last atlas directory that was used
        /// </summary>
        private string AtlasLastDirectory { get; set; } = Alphaleonis.Win32.Filesystem.Directory.GetCurrentDirectory();
        #endregion
        /// <summary>
        /// Using the selected col/rows and atlas image, deconstructs the image and creates a folder with the same name
        /// as the origin image containing the individual frames
        /// </summary>

        internal void ConvertAtlasToFrames()
        {

        }

        /// <summary>
        /// User selects atlas sheet to deconstruct into individual frames
        /// </summary>
        internal void SelectSheetInput()
        {
            using (var selectedFileDialog = new OpenFileDialog()
            {
                Filter = $"Image Files|*.png;*.dds",
                Title = "Select Atlas sheet for deconstruction",
                CheckFileExists = true,
                InitialDirectory = AtlasLastDirectory
            })
            {
                var selection = selectedFileDialog.ShowDialog();
                if (selection == DialogResult.OK)
                {
                    InputSheetFileSelection = selectedFileDialog.FileName;
                    var info = new DirectoryInfo(selectedFileDialog.FileName);
                    AtlasLastDirectory = info.Parent.FullName;
                }
            }
        }

        /// <summary>
        /// User selects the folder to deposit the individual frames into
        /// </summary>
        internal void SelectFramesOutput()
        {
            var selectedFolderDialog = new VistaFolderBrowserDialog()
            {
                SelectedPath = AtlasLastDirectory,
                UseDescriptionForTitle = true,
                Description = "Select folder to save atlas frames to"
            };

            var selection = selectedFolderDialog.ShowDialog();
            if (selection == true)
            {
                OutputFolderSelectionForFrames = selectedFolderDialog.SelectedPath;
                AtlasLastDirectory = selectedFolderDialog.SelectedPath;
            }
        }
        #endregion

        #region Frames to Atlas
        #region Properties
        private int _horizontalFramesForSheet = 1;
        /// <summary>
        /// The number of frames wide to make the sheet; the height will automatically be calculated
        /// </summary>
        public int HorizontalFramesForSheet {
            get { return _horizontalFramesForSheet; }
            set { 
                _horizontalFramesForSheet = value;
                OnNotifyPropertyChanged();
            } 
        }

        private int _verticalFramesForSheet;
        /// <summary>
        /// The number of frames high to make the sheet, automatically calculated
        /// </summary>
        public int VerticalFramesForSheet
        {
            get { return _verticalFramesForSheet; }
            set
            {
                _verticalFramesForSheet = value;
                OnNotifyPropertyChanged();
            }
        }

        private List<string> SelectedFrames { get; set; }

        private string _inputFilesSelectionForSheet;
        /// <summary>
        /// The files to use to make the sheet
        /// </summary>
        public string InputFilesSelectionForSheet
        {
            get { return _inputFilesSelectionForSheet; }
            set
            {
                _inputFilesSelectionForSheet = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _outputFolderSelectionForSheet;
        /// <summary>
        /// The output folder to save the sheet in
        /// </summary>
        public string OutputFolderSelectionForSheet
        {
            get { return _outputFolderSelectionForSheet;}
            set
            {
                _outputFolderSelectionForSheet = value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion

        internal void SelectAtlasOutput()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Select the frames used to generate an atlas sheet
        /// </summary>
        internal void SelectFramesInput()
        {
            var selectedFilesDialog = new OpenFileDialog()
            {
                Multiselect = true
            };
            var selection = selectedFilesDialog.ShowDialog();
            if (selection == DialogResult.OK)
            {
                InputFilesSelectionForSheet = string.Join(", ", selectedFilesDialog.SafeFileNames);
                SelectedFrames = selectedFilesDialog.FileNames.ToList();
            }
        }

        internal void ConvertFramesToAtlas()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
