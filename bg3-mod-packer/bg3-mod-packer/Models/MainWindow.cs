using System.ComponentModel;

namespace bg3_mod_packer.Models
{
    public class MainWindow : INotifyPropertyChanged
    {
        private string _consoleOutput;
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
