/// <summary>
/// The game object viewmodel.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using bg3_modders_multitool.Models;
    using bg3_modders_multitool.Models.GameObjects;
    using bg3_modders_multitool.Models.GameObjectTypes;
    using bg3_modders_multitool.Models.Races;
    using bg3_modders_multitool.Models.StatStructures;
    using bg3_modders_multitool.Services;
    using HelixToolkit.Wpf.SharpDX;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media.Imaging;

    public class GameObjectViewModel : BaseViewModel
    {
        public GameObjectViewModel()
        {
            RootTemplateHelper = new RootTemplateHelper(this);

            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera() { FarPlaneDistance = 3000, FieldOfView = 75 };
            var matrix = new System.Windows.Media.Media3D.MatrixTransform3D(new System.Windows.Media.Media3D.Matrix3D()).Value;
            matrix.Translate(new System.Windows.Media.Media3D.Vector3D(0, 0, 0));
            matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), 180));
            Transform = new System.Windows.Media.Media3D.MatrixTransform3D(matrix);
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
        public Viewport3DX ViewPort { get; internal set; }
        public EffectsManager EffectsManager { get; }
        public Camera Camera { get; }

        private List<MeshGeometry3D> _meshList;

        public List<MeshGeometry3D> MeshList {
            get { return _meshList; }
            set {
                _meshList = value;
                OnNotifyPropertyChanged();
            }
        }

        private Visibility _modelLoading = Visibility.Hidden;

        public Visibility ModelLoading {
            get { return _modelLoading; }
            set {
                _modelLoading = value;
                OnNotifyPropertyChanged();
            }
        }

        private System.Windows.Media.Media3D.MatrixTransform3D _transform;

        public System.Windows.Media.Media3D.MatrixTransform3D Transform {
            get { return _transform; }
            set {
                _transform = value;
                OnNotifyPropertyChanged();
            }
        }

        private List<MeshGeometry> _meshFiles;

        public List<MeshGeometry> MeshFiles {
            get { return _meshFiles; }
            set {
                _meshFiles = value;
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
                var autoGenGameObject = new AutoGenGameObject(value.FileLocation, value.MapKey).Data;
                GameObjectAttributes = autoGenGameObject?.Attributes;
                GameObjectChildren = autoGenGameObject?.Children;
                var hasModel = GameObjectAttributes?.Any(goa => goa.Name == "CharacterVisualResourceID" || goa.Name == "VisualTemplate");
                if(hasModel == true)
                    ModelLoading = Visibility.Visible;
                Stats = RootTemplateHelper.StatStructures.FirstOrDefault(ss => ss.Entry == value.Stats);
                Icon = RootTemplateHelper.TextureAtlases.FirstOrDefault(ta => ta == null ? false : ta.Icons.Any(icon => icon.MapKey == Info.Icon))?.GetIcon(Info.Icon);

                // reset viewport items
                var modelsToRemove = ViewPort.Items.Where(i => i as MeshGeometryModel3D != null).ToList();
                foreach (var model in modelsToRemove)
                {
                    ViewPort.Items.Remove(model);
                }

                Task.Run(() => {
                    var type = (FixedString)GameObjectAttributes?.Single(goa => goa.Name == "Type").Value;
                    var characterVisualResourceId = (FixedString)GameObjectAttributes?.SingleOrDefault(goa => goa.Name == "CharacterVisualResourceID")?.Value ?? value.CharacterVisualResourceID;
                    var visualTemplate = (FixedString)GameObjectAttributes?.SingleOrDefault(goa => goa.Name == "VisualTemplate")?.Value ?? value.VisualTemplate;
                    // this should dynamically create meshes based on the number of objects, assemble them based on transforms
                    var slots = RenderedModelHelper.GetMeshes(type, characterVisualResourceId ?? visualTemplate, RootTemplateHelper.CharacterVisualBanks, RootTemplateHelper.VisualBanks, RootTemplateHelper.BodySetVisuals,
                        RootTemplateHelper.MaterialBanks, RootTemplateHelper.TextureBanks);
                    MeshFiles = slots.OrderBy(slot => slot.File).ToList();

                    // Loop through slots
                    foreach (var lodLevels in slots)
                    {
                        // TODO - need lod slider, selecting highest lod first
                        var lod = lodLevels.MeshList.First().Value;
                        foreach (var model in lod)
                        {
                            Application.Current.Dispatcher.Invoke(() => {
                                // items are traditional PBR
                                // albedo from BM
                                // MRAO from PM
                                // normals from NM
                                // for characters
                                // for skinned parts, the BM is unused for characters
                                // non - skin albedo from BM, hemoglobin / melanin / veins / yellowing HMVY, cavity / lips / eyes / ambient occlusion from CLEA
                                // skin removal, melanin removal, detail normal removal from CancelMSK
                                // blood mask, dirt mask, bruises mask from SkinSharedMSK
                                // lips makeup roughness, head occlusion from RoughnessMSK
                                var map = new PBRMaterial
                                {
                                    AlbedoMap = GeneralHelper.DDSToTextureStream(model.BaseMap),
                                    // RenderAlbedoMap = type != "character", //// non-skin
                                    NormalMap = GeneralHelper.DDSToTextureStream(model.NormalMap),
                                    RoughnessMetallicMap = GeneralHelper.DDSToTextureStream(model.MRAOMap)
                                };

                                var mesh = new MeshGeometryModel3D() { Geometry = model.MeshGeometry3D, Material = map, CullMode = SharpDX.Direct3D11.CullMode.Back, Transform = Transform };
                                ViewPort.Items.Add(mesh);
                            });
                        }
                    }

                    ModelLoading = Visibility.Hidden; 
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

        private List<GameObjectNode> _gameObjectChildren;
        public List<GameObjectNode> GameObjectChildren {
            get { return _gameObjectChildren; }
            set {
                _gameObjectChildren = value;
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
