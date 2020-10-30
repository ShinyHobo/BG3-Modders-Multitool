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
                PropertyChangedEvent("ConsoleOutput");
            }
        }

        private int _indexFileCount;

        public int IndexFileCount {
            get { return _indexFileCount; }
            set {
                _indexFileCount = value;
                PropertyChangedEvent("IndexFileCount");
            }
        }

        private int _indexFileTotal;

        public int IndexFileTotal {
            get { return _indexFileTotal; }
            set {
                _indexFileTotal = value;
                PropertyChangedEvent("IndexFileTotal");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void PropertyChangedEvent(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
