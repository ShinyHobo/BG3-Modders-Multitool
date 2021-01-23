/// <summary>
/// The general helper service.
/// </summary>
namespace bg3_modders_multitool.Services
{
    using bg3_modders_multitool.ViewModels;
    using System;

    public static class GeneralHelper
    {
        /// <summary>
        /// Writes text to the main window console.
        /// </summary>
        /// <param name="text">The text to output.</param>
        public static void WriteToConsole(string text)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => {
                ((MainWindow)System.Windows.Application.Current.MainWindow.DataContext).ConsoleOutput += text;
            });
        }
    }
}
