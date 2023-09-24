namespace bg3_modders_multitool.ViewModels.Utilities
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Services;
    using Ookii.Dialogs.Wpf;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class AtlasToolViewModel : BaseViewModel
    {
        #region Atlas to Frames
        #region Properties
        public bool CanConvertToFrames => !string.IsNullOrEmpty(InputSheetFileSelection) && !string.IsNullOrEmpty(OutputFolderSelectionForFrames);

        private int _horizontalFramesInSheet = 1;
        /// <summary>
        /// The number of frames wide the sheet is
        /// </summary>
        public int HorizontalFramesInSheet { 
            get { return _horizontalFramesInSheet; }
            set
            {
                _horizontalFramesInSheet = value;
                if (sheetInputWidth != 0 && value != 0)
                    SheetOutputWidth = sheetInputWidth / value;
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
                if(sheetInputHeight != 0 && value != 0)
                    SheetOutputHeight = sheetInputHeight / value;
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
                var ext = Path.GetExtension(value);
                if (ext == ".png")
                {
                    using (var dimms = Image.FromFile(value))
                    {
                        sheetInputHeight = dimms.Height;
                        sheetInputWidth = dimms.Width;
                    }
                }
                else
                {
                    try
                    {
                        using (var image = Pfim.Pfimage.FromFile(InputSheetFileSelection))
                        {
                            sheetInputHeight = image.Height;
                            sheetInputWidth = image.Width;
                        }
                    }
                    catch(Exception ex)
                    {
                        GeneralHelper.WriteToConsole($"{ex.Message}\n{ex.StackTrace}");
                    }
                }

                SheetOutputWidth = sheetInputWidth / HorizontalFramesInSheet;
                SheetOutputHeight = sheetInputHeight / VerticalFramesInSheet;

                SheetInputDimensions = string.Format(Properties.Resources.InputImageDimensions, sheetInputWidth, sheetInputHeight);
                OnNotifyPropertyChanged();
                OnNotifyPropertyChanged("CanConvertToFrames");
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
                OnNotifyPropertyChanged("CanConvertToFrames");
            }
        }

        private int sheetInputHeight;
        private int sheetInputWidth;

        private int _sheetOutputHeight;
        public int SheetOutputHeight
        {
            get { return _sheetOutputHeight; }
            set {
                _sheetOutputHeight = value;
                SheetOutputDimensions = string.Format(Properties.Resources.OutputImageDimensions, SheetOutputWidth, SheetOutputHeight);
                OnNotifyPropertyChanged();
            }
        }

        private int _sheetOutputWidth;
        public int SheetOutputWidth
        {
            get { return _sheetOutputWidth; }
            set
            {
                _sheetOutputWidth = value;
                SheetOutputDimensions = string.Format(Properties.Resources.OutputImageDimensions, SheetOutputWidth, SheetOutputHeight);
                OnNotifyPropertyChanged();
            }
        }

        private string _sheetInputDimensions;
        /// <summary>
        /// The width/height dimensions of the input sheet
        /// </summary>
        public string SheetInputDimensions
        {
            get { return _sheetInputDimensions; }
            set
            {
                _sheetInputDimensions = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _sheetOutputDimensions;
        /// <summary>
        /// The width/height dimensions of the output sheet
        /// </summary>
        public string SheetOutputDimensions
        {
            get { return _sheetOutputDimensions; }
            set
            {
                _sheetOutputDimensions = value;
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
            try
            {
                var ext = Path.GetExtension(InputSheetFileSelection);
                var name = Path.GetFileNameWithoutExtension(InputSheetFileSelection);
                var folderPath = Path.Combine(OutputFolderSelectionForFrames, name);

                GeneralHelper.WriteToConsole(Properties.Resources.DeconstructingAtlas, folderPath);

                // Create and/or clean the output directory
                Directory.CreateDirectory(folderPath);
                DirectoryInfo di = new DirectoryInfo(folderPath);
                foreach (FileInfo file in di.GetFiles()) file.Delete();

                if (ext == ".png")
                {
                    using (var img = Image.FromFile(InputSheetFileSelection))
                    {
                        DeconstructAtlas(img, folderPath);
                    }
                }
                else
                {
                    using (var dds = Pfim.Pfimage.FromFile(InputSheetFileSelection))
                    {
                        var data = Marshal.UnsafeAddrOfPinnedArrayElement(dds.Data, 0);
                        var bitmap = new Bitmap(dds.Width, dds.Height, dds.Stride, PixelFormat.Format32bppArgb, data);
                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Png);
                            DeconstructAtlas(bitmap, folderPath);
                        }
                        bitmap.Dispose();
                    }
                }
            }
            catch(Exception ex)
            {
                GeneralHelper.WriteToConsole(Properties.Resources.GeneralError, ex.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// User selects atlas sheet to deconstruct into individual frames
        /// </summary>
        internal void SelectSheetInput()
        {
            using (var selectedFileDialog = new OpenFileDialog()
            {
                Filter = $"{Properties.Resources.ImageFilesFilter}|*.png;*.dds",
                Title = Properties.Resources.AtlasDeconstructionSelectionTitle,
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
                Description = Properties.Resources.AtlasFramesSaveTitle
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
        public bool CanConvertToSheet => !string.IsNullOrEmpty(InputFilesSelectionForSheet) && !string.IsNullOrEmpty(OutputFolderSelectionForSheet);
        private int _horizontalFramesForSheet = 1;
        /// <summary>
        /// The number of frames wide to make the sheet; the height will automatically be calculated
        /// </summary>
        public int HorizontalFramesForSheet {
            get { return _horizontalFramesForSheet; }
            set { 
                _horizontalFramesForSheet = value;
                CalculateAtlasDimensions();
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

        private int _frameHeight;
        /// <summary>
        /// The source image height
        /// </summary>
        public int FrameHeight
        {
            get { return _frameHeight; }
            set
            {
                _frameHeight = value;
                OnNotifyPropertyChanged();
            }
        }

        private int _frameWidth;
        /// <summary>
        /// The source image width
        /// </summary>
        public int FrameWidth
        {
            get { return _frameWidth; }
            set
            {
                _frameWidth = value;
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
                OnNotifyPropertyChanged("CanConvertToSheet");
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
                OnNotifyPropertyChanged("CanConvertToSheet");
            }
        }

        private string _frameInputDimensions;
        /// <summary>
        /// The frame input dimenions
        /// </summary>
        public string FrameInputDimensions
        {
            get { return _frameInputDimensions; }
            set
            {
                _frameInputDimensions = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _frameOutputDimensions;
        /// <summary>
        /// The frame to atlas output dimensions
        /// </summary>
        public string FrameOutputDimensions
        {
            get { return _frameOutputDimensions; }
            set
            {
                _frameOutputDimensions = value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// Select the frames used to generate an atlas sheet
        /// </summary>
        internal void SelectFramesInput()
        {
            using (var selectedFilesDialog = new OpenFileDialog()
            {
                Filter = $"{Properties.Resources.ImageFilesFilter}|*.png",
                Title = Properties.Resources.AtlasFramesSelectTitle,
                CheckFileExists = true,
                InitialDirectory = AtlasLastDirectory,
                Multiselect = true
            })
            {
                var selection = selectedFilesDialog.ShowDialog();
                if (selection == DialogResult.OK)
                {
                    InputFilesSelectionForSheet = string.Join(", ", selectedFilesDialog.SafeFileNames);
                    SelectedFrames = selectedFilesDialog.FileNames.ToList();
                    SelectedFrames.Sort();
                    var info = new DirectoryInfo(selectedFilesDialog.FileName);
                    AtlasLastDirectory = info.Parent.FullName;

                    var dimms = ValidateFrameUniformity();
                    if(dimms.Width > 0 && dimms.Height > 0)
                    {
                        FrameWidth = dimms.Width;
                        FrameHeight = dimms.Height;
                        FrameInputDimensions = string.Format(Properties.Resources.InputImageDimensions, FrameWidth, FrameHeight);
                        CalculateAtlasDimensions();
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the vertical frames for the sheet and updates the view
        /// </summary>
        private void CalculateAtlasDimensions()
        {
            VerticalFramesForSheet = (int)Math.Ceiling((float)SelectedFrames.Count / HorizontalFramesForSheet);
            FrameOutputDimensions = string.Format(Properties.Resources.OutputImageDimensions, FrameWidth * HorizontalFramesForSheet, FrameHeight * VerticalFramesForSheet);
        }

        /// <summary>
        /// Select the directory and enter the file name to be used for the generated atlas sheet
        /// </summary>
        internal void SelectAtlasOutput()
        {
            var selectedFileDialog = new SaveFileDialog()
            {
                InitialDirectory = AtlasLastDirectory,
                Title = Properties.Resources.AtlasFileSaveTitle,
                Filter = $"*.png|*.png"
            };

            var selection = selectedFileDialog.ShowDialog();
            if (selection == DialogResult.OK)
            {
                OutputFolderSelectionForSheet = selectedFileDialog.FileName;
                var info = new DirectoryInfo(selectedFileDialog.FileName);
                AtlasLastDirectory = info.Parent.FullName;
            }
        }

        /// <summary>
        /// Convert the selected files to a single png sheet
        /// </summary>
        internal void ConvertFramesToAtlas()
        {
            try
            {
                var ext = Path.GetExtension(OutputFolderSelectionForSheet);

                GeneralHelper.WriteToConsole(Properties.Resources.ConstructingAtlas);

                var atlasWidth = FrameWidth * HorizontalFramesForSheet;
                var atlasHeight = FrameHeight * VerticalFramesForSheet;
                using (var img = new Bitmap(atlasWidth, atlasHeight))
                {
                    var imgGfx = Graphics.FromImage(img);
                    for (int i = 0; i < VerticalFramesForSheet; i++)
                    {
                        for (int j = 0; j < HorizontalFramesForSheet; j++)
                        {
                            var index = i * HorizontalFramesForSheet + j;
                            if (index < SelectedFrames.Count)
                            {
                                var file = SelectedFrames[index];
                                using (var frame = Image.FromFile(file))
                                {
                                    imgGfx.DrawImage(frame, new Rectangle(j * FrameWidth, i * FrameHeight, FrameWidth, FrameHeight), new Rectangle(0, 0, FrameWidth, FrameHeight), GraphicsUnit.Pixel);
                                    frame.Dispose();
                                }
                            }
                        }
                    }
                    imgGfx.Dispose();
                    img.Save(OutputFolderSelectionForSheet, ImageFormat.Png);
                }

                GeneralHelper.WriteToConsole(Properties.Resources.AtlasConstructed);
            }
            catch (Exception ex)
            {
                GeneralHelper.WriteToConsole(Properties.Resources.GeneralError, ex.Message, ex.StackTrace);
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Pulls images out of a given atlas bitmap and saves the individual frames
        /// </summary>
        /// <param name="bitmap">The atlas bitmap</param>
        /// <param name="saveLocation">The location to save the frames</param>
        private void DeconstructAtlas(Image image, string saveLocation)
        {
            var imageNum = 0;
            for (int i = 0; i < VerticalFramesInSheet; i++)
            {
                for (int j = 0; j < HorizontalFramesInSheet; j++)
                {
                    var frame = new Bitmap(SheetOutputWidth, SheetOutputHeight);
                    var gfx = Graphics.FromImage(frame);
                    gfx.DrawImage(image, new Rectangle(0, 0, SheetOutputWidth, SheetOutputHeight), new Rectangle(j * SheetOutputWidth, i * SheetOutputHeight, SheetOutputWidth, SheetOutputHeight), GraphicsUnit.Pixel);
                    gfx.Dispose();
                    if (!CheckIfTransparent(frame))
                    {
                        frame.Save(Path.Combine(saveLocation, $"{imageNum.ToString().PadLeft(4, '0')}.png"), ImageFormat.Png);
                    }
                    frame.Dispose();
                    imageNum++;
                }
            }
            GeneralHelper.WriteToConsole(Properties.Resources.AtlasDeconstructed);
        }

        /// <summary>
        /// Check if image is completely transparent
        /// https://stackoverflow.com/a/53688608
        /// </summary>
        /// <param name="bitmap">The image to check</param>
        /// <returns>Whether or not the image is completely transparent</returns>
        private static bool CheckIfTransparent(Bitmap bitmap)
        {
            // Not an alpha-capable color format. Note that GDI+ indexed images are alpha-capable on the palette.
            if (((ImageFlags)bitmap.Flags & ImageFlags.HasAlpha) == 0)
                return false;
            // Indexed format, and no alpha colours in the image's palette: immediate pass.
            if ((bitmap.PixelFormat & PixelFormat.Indexed) != 0 && bitmap.Palette.Entries.All(c => c.A == 255))
                return false;
            // Get the byte data 'as 32-bit ARGB'. This offers a converted version of the image data without modifying the original image.
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Int32 len = bitmap.Height * data.Stride;
            Byte[] bytes = new Byte[len];
            Marshal.Copy(data.Scan0, bytes, 0, len);
            bitmap.UnlockBits(data);
            // Check the alpha bytes in the data. Since the data is little-endian, the actual byte order is [BB GG RR AA]
            for (Int32 i = 3; i < len; i += 4)
                if (bytes[i] != 0)
                    return false;
            return true;
        }

        /// <summary>
        /// Validates that all input images are the same size.
        /// </summary>
        /// <returns>The image size. Returns 0,0 if not uniform</returns>
        private (int Width, int Height) ValidateFrameUniformity()
        {
            var height = 0;
            var width = 0;
            var diffDimms = false;

            foreach (var file in SelectedFrames)
            {
                var ext = Path.GetExtension(file);
                var name = Path.GetFileNameWithoutExtension(file);
                var imgHeight = 0;
                var imgWidth = 0;
                if (ext == ".png")
                {
                    using (var img = Image.FromFile(file))
                    {
                        imgHeight = img.Height;
                        imgWidth = img.Width;
                    }
                }
                else
                {
                    using (var dds = Pfim.Pfimage.FromFile(file))
                    {
                        imgHeight = dds.Height;
                        imgWidth = dds.Width;
                    }
                }
                if (height == 0)
                    height = imgHeight;
                if (width == 0)
                    width = imgWidth;
                diffDimms = width != imgWidth || height != imgHeight;
                if (diffDimms)
                    break;
            }

            if (diffDimms)
            {
                GeneralHelper.WriteToConsole(Properties.Resources.SourceFramesAreNotUniform);
                return (0, 0);
            }
            return (width, height);
        }
        #endregion
    }
}
