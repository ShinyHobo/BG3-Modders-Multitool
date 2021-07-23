/// <summary>
/// The general helper service.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using Alphaleonis.Win32.Filesystem;
    using bg3_modders_multitool.ViewModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    public static class GeneralHelper
    {
        /// <summary>
        /// Writes text to the main window console.
        /// </summary>
        /// <param name="text">The text to output.</param>
        public static void WriteToConsole(string text)
        {
            Application.Current.Dispatcher.Invoke(() => {
                ((MainWindow)Application.Current.MainWindow.DataContext).ConsoleOutput += text;
            });
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
                var el = VisualTreeHelper.GetChild(parent, i) as UIElement;
                if (el == null) continue;

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
                var toggleText = setting ? "on" : "off";
                GeneralHelper.WriteToConsole($"Quick launch settings toggled {toggleText}!\n");
            }
        }
    }
}
