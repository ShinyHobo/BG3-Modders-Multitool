namespace bg3_modders_multitool
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
  
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private App()
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(
                typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
        }

        public static void Log(string logMessage, TextWriter w)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            w.WriteLine("  :");
            w.WriteLine($"  :{logMessage}");
            w.WriteLine("-------------------------------");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            // Sets the translation to use
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-CN");

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                Log($"An Error occured:\nStack Trace:\n{e.Exception.StackTrace}\n\nMessage: {e.Exception.Message}", w);
            }

            // Let exception propagate
            e.Handled = false;
        }
    }
}