namespace bg3_mod_packer.ViewModels
{
    using bg3_mod_packer.Models;
    using bg3_mod_packer.Services;
    using System.Collections.ObjectModel;

    public class GameObjectViewModel : BaseViewModel
    {
        public GameObjectViewModel()
        {
            RootTemplateHelper = new RootTemplateHelper();
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

    }
}
