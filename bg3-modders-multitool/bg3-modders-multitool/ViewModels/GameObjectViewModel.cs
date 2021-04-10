﻿/// <summary>
/// The game object viewmodel.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using bg3_modders_multitool.Models;
    using bg3_modders_multitool.Models.GameObjects;
    using bg3_modders_multitool.Models.Races;
    using bg3_modders_multitool.Models.StatStructures;
    using bg3_modders_multitool.Services;
    using HelixToolkit.Wpf.SharpDX;
    using HelixToolkit.Wpf.SharpDX.Assimp;
    using HelixToolkit.Wpf.SharpDX.Model.Scene;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;

    public class GameObjectViewModel : BaseViewModel
    {
        public GameObjectViewModel()
        {
            RootTemplateHelper = new RootTemplateHelper(this);

            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera();
            Material = PhongMaterials.LightGray;
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

        /// <summary>
        /// Finds a given gameobject by its mapkey.
        /// </summary>
        /// <param name="mapKey">The mapkey to search for.</param>
        /// <returns>The gameobject.</returns>
        public GameObject FindGameObject(string mapKey)
        {
            foreach (var gameObject in RootTemplateHelper.GameObjects)
            {
                var found = SearchGameObject(gameObject, mapKey);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Searches through gameobject and children for a given mapkey.
        /// </summary>
        /// <param name="gameObject">The gameobject to search on.</param>
        /// <param name="mapKey">The mapkey to find.</param>
        /// <returns>The matching gameobject.</returns>
        private GameObject SearchGameObject(GameObject gameObject, string mapKey)
        {
            if (gameObject == null)
                return null;
            if (gameObject.MapKey == mapKey)
                return gameObject;
            foreach (var child in gameObject.Children)
            {
                var found = SearchGameObject(child, mapKey);
                if (found != null)
                    return found;
            }
            return null;
        }

        #region SharpDX
        public EffectsManager EffectsManager { get; }
        public Camera Camera { get; }
        public Material Material { get; }

        private Geometry3D _mesh;
        public Geometry3D Mesh { 
            get { return _mesh; } 
            set {
                _mesh = value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion

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
                GameObjectAttributes = new AutoGenGameObject(value.FileLocation, value.MapKey).Data?.Attributes;
                Stats = RootTemplateHelper.StatStructures.FirstOrDefault(ss => ss.Entry == value.Stats);
                Icon = RootTemplateHelper.TextureAtlases.FirstOrDefault(ta => ta == null ? false : ta.Icons.Any(icon => icon.MapKey == Info.Icon))?.GetIcon(Info.Icon);

                //var importFormats = Importer.SupportedFormats;
                //var exportFormats = HelixToolkit.Wpf.SharpDX.Assimp.Exporter.SupportedFormats;

                Task.Run(() => {
                    // Get model for loaded object (.GR2)
                    var filename = @"J:\BG3\bg3-modders-multitool\bg3-modders-multitool\bg3-modders-multitool\bin\x64\Debug\UnpackedData\Models\Public\Shared\Assets\Characters\_Models\_Creatures\Dragon_Red\Dragon_Red_A";
                    if(!File.Exists($"{filename}.dae"))
                    {
                        // convert .GR2 file to .dae with divine.exe (get rid of skeleton?)
                    }
                    var importer = new Importer();
                    // Update material here?
                    var file = importer.Load($"{filename}.dae");
                    // Get correct item meshnode (multiple might be due to skeleton export)
                    var meshNode = file.Root.Items[0].Items[1] as MeshNode;
                    var meshGeometry = meshNode.Geometry as MeshGeometry3D;
                    meshGeometry.Normals = meshGeometry.CalculateNormals();
                    Mesh = meshGeometry;
                    importer.Dispose();
                });

                OnNotifyPropertyChanged();
            }
        }

        private List<GameObjectAttribute> _gameObjectAttributes;

        public List<GameObjectAttribute> GameObjectAttributes {
            get { return _gameObjectAttributes; }
            set {
                _gameObjectAttributes = value;
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

        private bool _loaded = false;

        public bool Loaded {
            get { return _loaded; }
            set {
                _loaded = true;
                OnNotifyPropertyChanged();
            }
        }

        public string SelectedKey { get; set; }
        #endregion
    }
}
