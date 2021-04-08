/// <summary>
/// The general helper service.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.ViewModels;
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
    }
}
