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
                    case "int32":
                        type = "int";
                        break;
                    case "uint8":
                        type = "byte";
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
    }
}
