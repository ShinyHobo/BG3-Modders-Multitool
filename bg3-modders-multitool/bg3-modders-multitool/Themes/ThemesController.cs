using System;
using System.Windows;
using System.Windows.Media;

namespace bg3_modders_multitool.Themes {
    public static class ThemesController {
        public static ThemeType CurrentTheme { get; set; }

        private static ResourceDictionary ThemeDictionary {
            get => Application.Current.Resources.MergedDictionaries[0];
            set => Application.Current.Resources.MergedDictionaries[0] = value;
        }

        private static ResourceDictionary ControlColours {
            get => Application.Current.Resources.MergedDictionaries[1];
            set => Application.Current.Resources.MergedDictionaries[1] = value;
        }

        private static ResourceDictionary Controls {
            get => Application.Current.Resources.MergedDictionaries[2];
            set => Application.Current.Resources.MergedDictionaries[2] = value;
        }

        public static void SetTheme(ThemeType theme) {
            string themeName = theme.GetName();
            if (string.IsNullOrEmpty(themeName)) {
                return;
            }

            CurrentTheme = theme;
            ThemeDictionary = new ResourceDictionary() { Source = new Uri($"Themes/ColorDictionaries/{themeName.Replace(" ", string.Empty)}.xaml", UriKind.Relative) };
            ControlColours = new ResourceDictionary() { Source = new Uri("Themes/ControlColors.xaml", UriKind.Relative) };
            Controls = new ResourceDictionary() { Source = new Uri("Themes/Controls.xaml", UriKind.Relative) };
        }

        public static object GetResource(object key) {
            return ThemeDictionary[key];
        }

        public static SolidColorBrush GetBrush(string name) {
            return GetResource(name) is SolidColorBrush brush ? brush : new SolidColorBrush(Colors.White);
        }
    }
}