namespace bg3_mod_packer.ViewModels
{
    using bg3_mod_packer.Models;
    using bg3_mod_packer.Services;
    using System.Collections.ObjectModel;
    using System.Windows.Controls;

    public class GameObjectViewModel : BaseViewModel
    {
        public GameObjectViewModel()
        {
            RootTemplateHelper = new RootTemplateHelper();
        }

        public void Clear()
        {
            RootTemplateHelper.Clear();
        }

        public RootTemplateHelper RootTemplateHelper;

        private ObservableCollection<GameObject> _gameObjects;

        public ObservableCollection<GameObject> GameObjects {
            get { return _gameObjects;  }
            set {
                _gameObjects = value;
                OnNotifyPropertyChanged();
            }
        }

        private GameObject _info;

        public GameObject Info {
            get { return _info; }
            set {
                _info = value;
                OnNotifyPropertyChanged();
            }
        }

        public Button DisabledButton { get; set; }
    }
}
