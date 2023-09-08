namespace bg3_modders_multitool.ViewModels.Utilities
{
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
        #endregion

        internal void ConvertAtlasToFrames()
        {
            throw new NotImplementedException();
        }

        internal void ConvertFramesToAtlas()
        {
            throw new NotImplementedException();
        }

        internal void SelectSheetInput()
        {
            var selectedFileDialog = new OpenFileDialog()
            {
                
            };
            var selection = selectedFileDialog.ShowDialog();
            if (selection == DialogResult.OK)
            {

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

        internal void SelectSheetsOutput()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
