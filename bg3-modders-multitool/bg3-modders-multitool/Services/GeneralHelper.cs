/// <summary>
/// The general helper service.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.ViewModels;
    using System.Windows;

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
        /// Locates the first UI element of a given type recursively.
        /// </summary>
        /// <typeparam name="T">The type of object to return</typeparam>
        /// <param name="obj">The object to search</param>
        /// <returns>The object</returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj != null)
            {
                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(obj); i++)
                {
                    var child = System.Windows.Media.VisualTreeHelper.GetChild(obj, i);
                    if (child is T)
                    {
                        return (T)child;
                    }

                    T childItem = FindVisualChild<T>(child);
                    if (childItem != null) return childItem;
                }
            }
            return null;
        }
    }
}
