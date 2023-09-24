namespace bg3_modders_multitool.Themes {
    using System;

    public enum ThemeType {
        SoftDark,
        RedBlackTheme,
        DeepDark,
        GreyTheme,
        DarkGreyTheme,
        LightTheme,
    }

    public static class ThemeTypeExtension {
        public static string GetName(this ThemeType type) {
            switch (type) {
                case ThemeType.SoftDark:        return "Obscured";
                case ThemeType.RedBlackTheme:   return "Astarion";
                case ThemeType.DeepDark:        return "Underdark";
                case ThemeType.GreyTheme:       return "Grey";
                case ThemeType.DarkGreyTheme:   return "Dark Grey";
                case ThemeType.LightTheme:   return "Bright Light";
                default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}