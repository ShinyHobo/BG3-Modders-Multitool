/// <summary>
/// The game object viewmodel.
/// </summary>
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

        /// <summary>
        /// Filters the game objects.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>The filtered list of game objects.</returns>
        public ObservableCollection<GameObject> Filter(string filter)
        {
            if(string.IsNullOrEmpty(filter))
            {
                return UnfilteredGameObjects;
            }
            else
            {
                var filteredList = new ObservableCollection<GameObject>();
                foreach (var gameObject in UnfilteredGameObjects)
                {
                    var filterItem = gameObject.Search(filter);
                    if (filterItem != null)
                        filteredList.Add(filterItem);
                }
                return filteredList;
            }
        }

        #region Properties
        public RootTemplateHelper RootTemplateHelper;

        private ObservableCollection<GameObject> _unfilteredGameObjects;

        public ObservableCollection<GameObject> UnfilteredGameObjects {
            get { return _unfilteredGameObjects; }
            set {
                _unfilteredGameObjects = value;
                OnNotifyPropertyChanged();
            }
        }

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
        #endregion
    }
}
