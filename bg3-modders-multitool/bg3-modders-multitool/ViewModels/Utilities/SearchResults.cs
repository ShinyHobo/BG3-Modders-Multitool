/// <summary>
/// The searcher view model.
/// </summary>
namespace bg3_modders_multitool.ViewModels
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Services;
    using HelixToolkit.Wpf.SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// The model for search results
    /// </summary>
    public class SearchResults : BaseViewModel
    {
        public SearchResults()
        {
            IndexHelper = new IndexHelper { DataContext = this };
            PakUnpackHelper = new PakUnpackHelper { DataContext = this };
            IsIndexing = false;
            EffectsManager = new DefaultEffectsManager();
            Camera = new PerspectiveCamera() { FarPlaneDistance = 3000, FieldOfView = 75 };
            Material = PhongMaterials.LightGray;
            var matrix = new System.Windows.Media.Media3D.MatrixTransform3D(new System.Windows.Media.Media3D.Matrix3D()).Value;
            matrix.Translate(new System.Windows.Media.Media3D.Vector3D(0, 0, 0));
            matrix.Rotate(new System.Windows.Media.Media3D.Quaternion(new System.Windows.Media.Media3D.Vector3D(0, 1, 0), 180));
            Transform = new System.Windows.Media.Media3D.MatrixTransform3D(matrix);
            AllowInteraction = true;
        }

        public void Clear()
        {
            IndexHelper.Clear();
        }

        /// <summary>
        /// Gets the time that has passed since indexing began.
        /// </summary>
        /// <returns>The time taken.</returns>
        public TimeSpan GetTimeTaken()
        {
            return TimeSpan.FromTicks(DateTime.Now.Subtract(IndexStartTime).Ticks);
        }

        #region Properties

        #region Indexing
        public IndexHelper IndexHelper { get; set; }
        public PakUnpackHelper PakUnpackHelper { get; set; }

        public bool Extracting { get; set; }

        private int _resultTotal;

        public int IndexFileTotal {
            get { return _resultTotal; }
            set {
                _resultTotal = value;
                _lastUpdate = DateTime.Now;
                OnNotifyPropertyChanged();
            }
        }

        private DateTime _lastUpdate;
        private int _resultCount;

        public int IndexFileCount {
            get { return _resultCount; }
            set {
                _resultCount = value;
                if((DateTime.Now - _lastUpdate).TotalMilliseconds > 100 || _resultCount == IndexFileTotal || _resultCount == 0)
                {
                    // TODO - move time remaining calculation to timer thread
                    var timeTaken = GetTimeTaken();
                    var timeRemaining = ((timeTaken.TotalMinutes / IndexFileCount) * IndexFileTotal) - timeTaken.TotalMinutes;
                    if (timeRemaining < TimeSpan.MaxValue.TotalMinutes)
                    {
                        IndexTimeRemaining = TimeSpan.FromMinutes(timeRemaining);
                    }
                    _lastUpdate = DateTime.Now;
                    OnNotifyPropertyChanged();
                }
            }
        }

        private ObservableCollection<SearchResult> _results;

        public ObservableCollection<SearchResult> Results {
            get { return _results; }
            set {
                _results = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _selectAllToggled;
        public bool SelectAllToggled { 
            get { return _selectAllToggled; }
            set { 
                _selectAllToggled = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _allowInteraction;
        public bool AllowInteraction {
            get { return _allowInteraction; }
            set {
                _allowInteraction = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _leadingWildcardDisabled;

        /// <summary>
        /// Whether or not to utilize a leading wildcard for searches. Disabling this provides faster, but potentially unexpected results
        /// </summary>
        public bool LeadingWildcardDisabled
        {
            get { return _leadingWildcardDisabled; }
            set
            {
                _leadingWildcardDisabled = value;
                OnNotifyPropertyChanged();
            }
        }

        private DateTime _indexStartTime;

        public DateTime IndexStartTime {
            get { return _indexStartTime; }
            set {
                _indexStartTime = value;
                OnNotifyPropertyChanged();
            }
        }

        private TimeSpan _indexTimeRemaining;

        public TimeSpan IndexTimeRemaining {
            get { return _indexTimeRemaining; }
            set {
                _indexTimeRemaining = value;
                OnNotifyPropertyChanged();
            }
        }

        private Visibility _indexingVisibility;

        public Visibility IndexingVisibility {
            get { return _indexingVisibility; }
            set {
                _indexingVisibility = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _allowIndexing;

        public bool AllowIndexing {
            get { return _allowIndexing; }
            set {
                _allowIndexing = value && !string.IsNullOrEmpty(((MainWindow)Application.Current.MainWindow.DataContext)?.Bg3ExeLocation);
                OnNotifyPropertyChanged();
            }
        }

        private bool _isIndexing;

        public bool IsIndexing {
            get { return _isIndexing; }
            set {
                _isIndexing = value;
                IndexingVisibility = value ? Visibility.Visible : Visibility.Hidden;
                AllowIndexing = !value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion

        #region Search Result
        private ObservableCollection<SearchResult> _fileContents;

        public ObservableCollection<SearchResult> FileContents {
            get { return _fileContents; }
            set {
                _fileContents = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _selectedPath;

        public string SelectedPath {
            get { return _selectedPath; }
            set {
                _selectedPath = value;
                OnNotifyPropertyChanged();
            }
        }
        #endregion

        #region SharpDX
        /// <summary>
        /// Renders the model if one is present.
        /// </summary>
        /// <returns>Whether the selected path was a .gr2 file.</returns>
        public bool RenderModel()
        {
            ModelLoading = Visibility.Hidden;
            ModelVisible = Visibility.Hidden;
            if (FileHelper.IsGR2(SelectedPath))
            {
                ModelLoading = Visibility.Visible;
                var modelsToRemove = ViewPort.Items.Where(i => i as MeshGeometryModel3D != null).ToList();
                foreach (var model in modelsToRemove)
                {
                    ViewPort.Items.Remove(model);
                }

                Task.Run(() => {
                    var mesh = RenderedModelHelper.GetMesh(Path.ChangeExtension(FileHelper.GetPath(SelectedPath), null), new Dictionary<string, Tuple<string, string>>(),
                        new Dictionary<string, string>(), new Dictionary<string, string>(), new Dictionary<string, string>()); // skipping materials to speed load times for searches
                    if (mesh != null && mesh.Any())
                    {
                        var lod = mesh.First().Value;
                        if (lod.Any())
                        {
                            foreach (var model in lod)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    var meshGeometry = new MeshGeometryModel3D() { Geometry = model.MeshGeometry3D, Material = Material, CullMode = SharpDX.Direct3D11.CullMode.Back, Transform = Transform };
                                    ViewPort.Items.Add(meshGeometry);
                                });
                            }
                            ModelVisible = Visibility.Visible;
                        }
                    }
                    ModelLoading = Visibility.Hidden;
                });
                return true;
            }
            return false;
        }

        private Visibility _modelLoading = Visibility.Hidden;

        public Visibility ModelLoading {
            get { return _modelLoading; }
            set {
                _modelLoading = value;
                OnNotifyPropertyChanged();
            }
        }

        private Visibility _modelVisible = Visibility.Hidden;

        public Visibility ModelVisible {
            get { return _modelVisible; }
            set {
                _modelVisible = value;
                OnNotifyPropertyChanged();
            }
        }

        public Viewport3DX ViewPort { get; internal set; }
        public EffectsManager EffectsManager { get; }
        public Camera Camera { get; }

        private Material _material;

        public Material Material {
            get { return _material; }
            set {
                _material = value;
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

        public List<string> FullResultList { get; internal set; }
        #endregion

        #endregion
    }

    /// <summary>
    /// The model for a single search result.
    /// </summary>
    public class SearchResult : BaseViewModel
    {
        private string _path;

        public string Path {
            get { return _path; }
            set {
                _path = value;
                OnNotifyPropertyChanged();
            }
        }

        private long _key;

        public long Key {
            get { return _key; }
            set {
                _key = value;
                OnNotifyPropertyChanged();
            }
        }

        private string _text;

        public string Text {
            get { return _text; }
            set {
                _text = value;
                OnNotifyPropertyChanged();
            }
        }

        private bool _selected;

        public bool Selected { 
            get { return _selected; } 
            set {
                _selected = value;
                OnNotifyPropertyChanged();
            }
        }
    }
}
