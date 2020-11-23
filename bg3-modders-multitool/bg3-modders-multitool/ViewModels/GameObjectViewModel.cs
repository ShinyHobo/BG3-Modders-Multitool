/// <summary>
/// The game object viewmodel.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using bg3_modders_multitool.Models;
    using bg3_modders_multitool.Models.Races;
    using bg3_modders_multitool.Models.StatStructures;
    using bg3_modders_multitool.Services;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

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
                Race = RootTemplateHelper.Races.FirstOrDefault(race => race.UUID == value.RaceUUID);
                Stats = RootTemplateHelper.StatStructures.FirstOrDefault(ss => ss.Entry == value.Stats);
                Icon = RootTemplateHelper.TextureAtlases.Single(ta => ta.Path.Contains(value.Pak)).GetIcon(Info.Icon);
                OnNotifyPropertyChanged();
            }
        }

        private StatStructure _stats;

        public StatStructure Stats {
            get { return _stats; }
            set {
                _stats = value;
                OnNotifyPropertyChanged();
            }
        }

        private BitmapImage _icon;

        public BitmapImage Icon {
            get { return _icon; }
            set {
                _icon = value;
                OnNotifyPropertyChanged();
            }
        }

        private Race _race;

        public Race Race {
            get { return _race; }
            set {
                _race = value;
                OnNotifyPropertyChanged();
            }
        }

        public Button DisabledButton { get; set; }
        #endregion
    }
}
