using System.Windows;

namespace bg3_modders_multitool.Themes.Attached {
    public static class CornerHelper {
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(CornerHelper), new PropertyMetadata(new CornerRadius(0)));

        public static void SetCornerRadius(DependencyObject element, CornerRadius value) {
            element.SetValue(CornerRadiusProperty, value);
        }

        public static CornerRadius GetCornerRadius(DependencyObject element) {
            return (CornerRadius) element.GetValue(CornerRadiusProperty);
        }
    }
}
