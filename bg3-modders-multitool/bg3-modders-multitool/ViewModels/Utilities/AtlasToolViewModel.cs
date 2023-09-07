namespace bg3_modders_multitool.ViewModels.Utilities
{
    using System;

    public class AtlasToolViewModel : BaseViewModel
    {
        #region Atlas to Frames
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

        internal void ConvertAtlasToFrames()
        {
            throw new NotImplementedException();
        }

        internal void ConvertFramesToAtlas()
        {
            throw new NotImplementedException();
        }

        internal void SelectAtlasInput()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Frames to Atlas
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

        internal void SelectAtlasOutput()
        {
            throw new NotImplementedException();
        }

        internal void SelectSheetsInput()
        {
            throw new NotImplementedException();
        }

        internal void SelectSheetsOutput()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
