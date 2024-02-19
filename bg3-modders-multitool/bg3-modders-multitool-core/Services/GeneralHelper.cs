/// <summary>
/// The general helper service.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.Properties;
    using bg3_modders_multitool.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public static class GeneralHelper
    {
        /// <summary>
        /// Writes text to the main window console.
        /// </summary>
        /// <param name="resource">The text to output.</param>
        /// <param name="args">The arguments to pass into the text</param>
        public static void WriteToConsole(string resource, params object[] args)
        {
            if(resource != null)
            {
                Application.Current.Dispatcher.Invoke(() => {
                    try
                    {
                        var message = string.Format(resource, args);
                        ((MainWindow)Application.Current.MainWindow.DataContext).WriteToConsole(string.Format(resource, args));
                    }
                    catch
                    {
                        try
                        {
                            ((MainWindow)Application.Current.MainWindow.DataContext).WriteToConsole($"{Properties.Resources.BadTranslation}: {resource}");
                        } catch { }
                    }
                });
            }
        }

        /// <summary>
        /// Locates the first UI element with a given UID recursively.
        /// </summary>
        /// <param name="parent">The object to search</param>
        /// <param name="uid">The UID to search for.</param>
        /// <returns>The object.</returns>
        public static UIElement FindUid(this DependencyObject parent, string uid)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            if (count == 0) return null;

            for (int i = 0; i < count; i++)
            {
                if (!(VisualTreeHelper.GetChild(parent, i) is UIElement el)) continue;

                if (el.Uid == uid) return el;

                el = el.FindUid(uid);
                if (el != null) return el;
            }
            return null;
        }

        /// <summary>
        /// Creates a file containing class properties for GameObjects. For development use.
        /// </summary>
        /// <param name="attributeClasses">The distinct list of ids and types</param>
        public static void ClassBuilder(List<Tuple<string, string>> attributeClasses)
        {
            FileHelper.SerializeObject(attributeClasses, "GameObjectAttributeClasses");
            var classList = string.Empty;
            foreach (var attribute in attributeClasses.OrderBy(at => at.Item1))
            {
                if (!char.IsLetter(attribute.Item1[0]))
                    continue;
                var type = string.Empty;

                switch (attribute.Item2)
                {
                    case "guid":
                        type = "Guid";
                        break;
                    case "bool":
                        type = "bool";
                        break;
                    case "float":
                        type = "float";
                        break;
                    case "double":
                        type = "double";
                        break;
                    case "int8":
                        type = "sbyte";
                        break;
                    case "int16":
                        type = "short";
                        break;
                    case "int":
                    case "int32":
                        type = "int";
                        break;
                    case "uint8":
                        type = "byte";
                        break;
                    case "uint16":
                        type = "uint16";
                        break;
                    case "uint32":
                        type = "uint";
                        break;
                    case "uint64":
                        type = "ulong";
                        break;
                    case "fvec2":
                        type = "Tuple<float, float>";
                        break;
                    case "fvec3":
                        type = "Tuple<float, float, float>";
                        break;
                    case "fvec4":
                        type = "Tuple<float, float, float, float>";
                        break;
                    case "FixedString":
                    case "LSString":
                    case "TranslatedString":
                        type = attribute.Item2;
                        break;
                    case "mat4x4":
                        type = "List<Tuple<float, float, float, float>>";
                        break;
                    default:
                        throw new Exception($"Attribute type not covered: {attribute.Item2}");
                }
                var camelCaseId = char.ToLowerInvariant(attribute.Item1[0]) + attribute.Item1.Substring(1);
                classList +=    $"private {type} _{camelCaseId};\n\n" +
                                $"public {type} {attribute.Item1} {{\n" +
                                $"\tget {{ return _{camelCaseId}; }}\n" +
                                $"\tset {{ _{camelCaseId} = value; }}\n" +
                                $"}}\n\n";

            }
            if (!Directory.Exists("Development"))
                Directory.CreateDirectory("Development");
            File.WriteAllText("Development/classList.txt", classList);
        }

        /// <summary>
        /// Larian replaced many types with a number. This converts that number back to the old type.
        /// </summary>
        /// <param name="type">The new type enum value.</param>
        /// <returns>The old type.</returns>
        public static string LarianTypeEnumConvert(string type)
        {
            switch (type)
            {
                case "1":
                    type = "uint8";
                    break;
                case "2":
                    type = "int16";
                    break;
                case "3":
                    type = "uint16";
                    break;
                case "4":
                    type = "int";
                    break;
                case "5":
                    type = "uint32";
                    break;
                case "6":
                    type = "float";
                    break;
                case "7":
                    type = "double";
                    break;
                case "11":
                    type = "fvec2";
                    break;
                case "12":
                    type = "fvec3";
                    break;
                case "13":
                    type = "fvec4";
                    break;
                case "18": // CustomPointTransform
                    type = "mat4x4";
                    break;
                case "19":
                    type = "bool";
                    break;
                case "22":
                    type = "FixedString";
                    break;
                case "23":
                    type = "LSString";
                    break;
                case "24":
                    type = "uint64";
                    break;
                case "27":
                    type = "int8";
                    break;
                case "28":
                    type = "TranslatedString";
                    break;
                case "31":
                    type = "guid";
                    break;
                case "32":
                    type = "int32";
                    break;
                default:
                    throw new Exception($"Type {type} not covered for conversion.");
            }
            return type;
        }

        /// <summary>
        /// Toggles the quick launch features.
        /// </summary>
        /// <param name="setting">Whether or not quick launch options should be enabled.</param>
        public static void ToggleQuickLaunch(bool setting)
        {
            if(Properties.Settings.Default.quickLaunch != setting)
            {
                Properties.Settings.Default.quickLaunch = setting;
                FileHelper.CreateDestroyQuickLaunchMod(setting);
                Properties.Settings.Default.Save();
                var toggleText = setting ? Properties.Resources.On : Properties.Resources.Off;
                WriteToConsole(Properties.Resources.QuickLaunchEnabled, toggleText);
            }
        }

        /// <summary>
        /// Toggles the thread unlock setting
        /// </summary>
        /// <param name="setting">Whether or not threads should be unlocked for parallel processing</param>
        public static void ToggleUnlockThreads(bool setting)
        {
            if (Properties.Settings.Default.unlockThreads != setting)
            {
                Properties.Settings.Default.unlockThreads = setting;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Converts a DDS texture into a usable stream for displaying on models.
        /// </summary>
        /// <param name="texturePath">The filepath to the texture file.</param>
        /// <returns>The texture stream</returns>
        public static System.IO.Stream DDSToTextureStream(string texturePath)
        {
            System.IO.Stream texture = null;
            if (File.Exists(texturePath))
            {
                try
                {
                    using (var image = Pfim.Pfimage.FromFile(texturePath))
                    {
                        var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                        var bitmap = new System.Drawing.Bitmap(image.Width, image.Height, image.Stride, System.Drawing.Imaging.PixelFormat.Format32bppArgb, data);
                        var bitmapImage = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                        texture = BitmapSourceToStream(bitmapImage);
                    }
                }
                catch { }
            }
            return texture;
        }

        /// <summary>
        /// Converts a bitmap source to a stream.
        /// </summary>
        /// <param name="writeBmp">The bitmap image.</param>
        /// <returns>The stream.</returns>
        public static System.IO.Stream BitmapSourceToStream(BitmapSource writeBmp)
        {
            System.IO.Stream stream = new System.IO.MemoryStream();
            BitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(writeBmp));
            enc.Save(stream);

            return stream;
        }


        [DllImport("user32.dll")]
        static extern IntPtr GetOpenClipboardWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// Gets the Process that's holding the clipboard
        /// https://stackoverflow.com/questions/12769264/openclipboard-failed-when-copy-pasting-data-from-wpf-datagrid/21311270#21311270
        /// </summary>
        /// <returns>A Process object holding the clipboard, or null</returns>
        public static Process ProcessHoldingClipboard()
        {
            Process theProc = null;

            IntPtr hwnd = GetOpenClipboardWindow();

            if (hwnd != IntPtr.Zero)
            {
                GetWindowThreadProcessId(hwnd, out uint processId);

                Process[] procs = Process.GetProcesses();
                foreach (Process proc in procs)
                {
                    IntPtr handle = proc.MainWindowHandle;

                    if (handle == hwnd)
                    {
                        theProc = proc;
                    }
                    else if (processId == proc.Id)
                    {
                        theProc = proc;
                    }
                }
            }

            return theProc;
        }

        /// <summary>
        /// Gets the app file version
        /// </summary>
        /// <returns>The app file version represented as #.#.#x</returns>
        public static string GetAppVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        /// <summary>
        /// Toggles whether or not to send new paks to the mods folder instead of zipping them
        /// </summary>
        /// <param name="setting">True for pak to mods, false to zip in same directory</param>
        internal static void TogglePakToMods(bool setting)
        {
            if (Properties.Settings.Default.pakToMods != setting)
            {
                Properties.Settings.Default.pakToMods = setting;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// If threading is not unlocked, this creates a max degree of parallelism equal to 75% of the processor count multiplied by two, rounded up (2 threads per processor)
        /// </summary>
        public static ParallelOptions ParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Settings.Default.unlockThreads ? -1 : Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 2.0)) };
    }
}
