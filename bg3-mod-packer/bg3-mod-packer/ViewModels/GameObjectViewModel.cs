/// <summary>
/// The game object viewmodel.
/// </summary>
namespace bg3_mod_packer.ViewModels
{
    using bg3_mod_packer.Models;
    using bg3_mod_packer.Services;
    using System;
    using System.Collections.Generic;
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
                foreach (var go in UnfilteredGameObjects)
                {
                    var filterItem = Search(filter, go);
                    if (filterItem != null)
                        filteredList.Add(filterItem);
                }
                return filteredList;
            }
        }

        /// <summary>
        /// Recursively searches through the game object's children to find matching object names.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="gameObject">The game object to search through.</param>
        /// <returns>The filtered game object.</returns>
        private GameObject Search(string filter, GameObject gameObject)
        {
            if (gameObject == null)
                return null;
            var filteredList = new List<GameObject>();
            foreach (var subItem in gameObject.Children)
            {
                var filterItem = Search(filter, subItem);
                if (filterItem != null)
                    filteredList.Add(filterItem);
            }
            var filterGo = gameObject.Clone();
            filterGo.Children = filteredList;
            if (FindMatch(filter, filterGo))
                return filterGo;
            else
            {
                foreach (var subItem in filterGo.Children)
                {
                    if (FindMatch(filter, subItem) || subItem.Children.Count > 0)
                    {
                        return filterGo;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Finds a match to a game object property value.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="gameObject">The game object to filter on.</param>
        /// <returns></returns>
        private bool FindMatch(string filter, GameObject gameObject)
        {
            return gameObject.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   gameObject.MapKey?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   gameObject.ParentTemplateId?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   gameObject.DisplayNameHandle?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   gameObject.DisplayName?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   gameObject.DescriptionHandle?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   gameObject.Description?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   gameObject.Icon?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                   gameObject.Stats?.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
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
