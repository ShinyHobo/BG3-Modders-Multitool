/// <summary>
/// The .pak selection window view model.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Services;
    using bg3_modders_multitool.ViewModels.Reusables;

    public class PakSelection : BaseViewModel
    {
        /// <summary>
        /// Creates a file list from the list of files checked off for unpacking.
        /// </summary>
        /// <param name="files"></param>
        public void CreateFileList(List<string> files)
        {
            var unpackedInfo = new DirectoryInfo(FileHelper.UnpackedDataPath);
            var unpackedDirs = unpackedInfo.GetDirectories().Select(x => $"{x.Name}.pak").ToList();
            DisclaimerVisible = unpackedDirs.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            PakList = new ObservableCollection<CheckBox>();
            foreach (string file in files)
            {
                var fileName = Path.GetFileName(file);
                var isMultipartNumber = fileName.Split('_').Select(v => int.TryParse(v.Split('.')[0], out var num)).Last();
                if (!isMultipartNumber)
                    PakList.Add(new CheckBox
                    {
                        Name = unpackedDirs.Contains(fileName) ? $"{fileName}*" : fileName,
                        IsSelected = false
                    });
            }
            PakList = new ObservableCollection<CheckBox>(PakList.OrderBy(pak => pak.Name));
        }

        /// <summary>
        /// Select/deselect all .paks
        /// </summary>
        /// <param name="select">True to select all; false to deselect all.</param>
        public void SelectAll(bool select)
        {
            foreach (var pak in PakList)
            {
                pak.IsSelected = select;
            }
        }

        #region Properties
        private ObservableCollection<CheckBox> _pakList;

        public ObservableCollection<CheckBox> PakList {
            get { return _pakList;  }
            set {
                _pakList = value;
                OnNotifyPropertyChanged();
            }
        }

        private Visibility _disclaimerVisible = Visibility.Collapsed;
        public Visibility DisclaimerVisible
        {
            get { return _disclaimerVisible; }
            set
            {
                _disclaimerVisible = value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion
    }
}
