/// <summary>
/// The .pak selection window model.
/// </summary>
namespace bg3_mod_packer.Models
{
    using System.Collections.ObjectModel;
    using bg3_mod_packer.ViewModels;
    public class PakSelection
    {
        public ObservableCollection<CheckBox> PakList { get; set; }
    }
}
