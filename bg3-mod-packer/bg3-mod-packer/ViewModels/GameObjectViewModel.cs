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
        /// <param name="go">The game object to search through.</param>
        /// <returns>The filtered game object.</returns>
        private GameObject Search(string filter, GameObject go)
        {
            if (go == null)
                return null;
            var filteredList = new List<GameObject>();
            foreach (var subItem in go.Children)
            {
                var filterItem = Search(filter, subItem);
                if (filterItem != null)
                    filteredList.Add(filterItem);
            }
            var filterGo = go.Clone();
            filterGo.Children = filteredList;
            if (filterGo.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return filterGo;
            else
            {
                foreach (var subItem in filterGo.Children)
                {
                    if (subItem.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0 || subItem.Children.Count > 0)
                    {
                        return filterGo;
                    }
                }
                return null;
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
