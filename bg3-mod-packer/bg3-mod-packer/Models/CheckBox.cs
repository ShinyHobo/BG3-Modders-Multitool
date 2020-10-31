/// <summary>
/// Model for checkboxes.
/// </summary>
namespace bg3_mod_packer.Models
{
    public class CheckBox : ViewModels.BaseViewModel
    {
        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnNotifyPropertyChanged();
            }
        }
    }
}
