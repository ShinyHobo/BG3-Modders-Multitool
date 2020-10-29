/// <summary>
/// The model for the main window instance.
/// </summary>
namespace bg3_mod_packer.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using bg3_mod_packer.Helpers;

    public class MainWindow : INotifyPropertyChanged
    {
        private string _consoleOutput;

        public PakUnpackHelper UnpackingProcess { get; internal set; }

        public string ConsoleOutput {
            get { return _consoleOutput; }
            set {
                _consoleOutput = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConsoleOutput"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
