/// <summary>
/// The .pak selection window model.
/// </summary>
namespace bg3_mod_packer.Models
{
    using System.Collections.ObjectModel;
    public class PakSelection
    {
        public ObservableCollection<CheckBox> PakList { get; set; }
    }
}
