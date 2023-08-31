using bg3_modders_multitool;
using System.Windows;
using System;
using bg3_modders_multitool.Views;

/// <summary>
/// Controls the application lifecycle to allow for on the fly language selection
/// </summary>
public class LocalizationController : Application
{
    [STAThread]
    public static void Main()
    {
        App app = new App();
        app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        MainWindow wnd = new MainWindow();
        wnd.Closed += Wnd_Closed;
        app.Run(wnd);
    }

    private static void Wnd_Closed(object sender, EventArgs e)
    {
        MainWindow wnd = sender as MainWindow;
        var dataContext = (bg3_modders_multitool.ViewModels.MainWindow)wnd.DataContext;
        if (!string.IsNullOrEmpty(dataContext.SelectedLanguage))
        {
            bg3_modders_multitool.Properties.Settings.Default.selectedLanguage = dataContext.SelectedLanguage;
            bg3_modders_multitool.Properties.Settings.Default.Save();

            wnd.Closed -= Wnd_Closed;

            wnd = new MainWindow();
            wnd.Closed += Wnd_Closed;
            wnd.Show();
        }
        else
        {
            App.Current.Shutdown();
        }
    }
}