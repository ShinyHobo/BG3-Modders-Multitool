using bg3_modders_multitool;
using System.Windows;
using System;
using bg3_modders_multitool.Views;
using CommandLine;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/// <summary>
/// Controls the application lifecycle to allow for on the fly language selection
/// </summary>
public class LocalizationController : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            var wnd = new bg3_modders_multitool.ViewModels.MainWindow();
            var app = new Application { MainWindow = new Window { DataContext = wnd }};

            AttachConsole(-1);

            Task.Run(() => Parser.Default.ParseArguments<Cli>(args)
                //.WithNotParsed(Cli.NotParsed)
                .WithParsedAsync(Cli.Run)
                .ContinueWith(_ => Console.WriteLine(wnd.ConsoleOutput))).Wait();

            FreeConsole();
        }
        else
        {
            MainWindow wnd = new MainWindow();
            wnd.Closed += Wnd_Closed;
            App app = new App();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            app.Run(wnd);
        }
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

    [DllImport("kernel32.dll")]
    static extern bool AttachConsole(int dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    /// <summary>
    /// The available cli arguments
    /// </summary>
    private class Cli
    {
        /// <summary>
        /// The source folder/file
        /// </summary>
        [Option('s', "source", Required = true, HelpText = "Input folder/file")]
        public string Source { get; set; }

        /// <summary>
        /// The destination folder/file
        /// </summary>
        [Option('d', "destination", Required = true, HelpText = "Output folder/file")]
        public string Destination { get; set; }

        /// <summary>
        /// The compression level to use (0-4)
        /// </summary>
        [Option('c', "compression", HelpText = "0: None\r\n1: LZ4\r\n2: LZ4 HC\r\n3: Zlib Fast\r\n4: Zlib Optimal")]
        public int Compression { get; set; }

        /// <summary>
        /// Whether to save as zip with usual contents, or leave as pak
        /// </summary>
        [Option('z', "zip", HelpText = "Whether to save as zip with usual contents, or leave as pak")]
        public bool Zip { get; set; }

        public string GetUsage()
        {
            return "Read wiki for usage instructions...";
        }

        /// <summary>
        /// Handle cli errors
        /// </summary>
        /// <param name="errs">The error list</param>
        public static void NotParsed(IEnumerable<Error> errs)
        {
            var result = -2;
            Console.WriteLine("errors {0}", errs.Count());
            if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
                result = -1;
            Console.WriteLine("Exit code {0}", result);
        }

        /// <summary>
        /// Runs the multitool cli commands
        /// </summary>
        /// <param name="options">The cli options</param>
        /// <returns>The task</returns>
        public static async Task Run(Cli options)
        {
            var source = Path.GetFullPath(options.Source);
            var destination = Path.GetFullPath(options.Destination);
            var compression = bg3_modders_multitool.ViewModels.MainWindow.AvailableCompressionTypes.FirstOrDefault(c => c.Id == options.Compression);
            var zip = options.Zip;

            // Check source (must exist)
            var sourceIsDirectory = System.IO.Path.GetExtension(source) == string.Empty;
            if ((sourceIsDirectory && !Directory.Exists(source)) || (!sourceIsDirectory && !File.Exists(source)))
            {
                Console.WriteLine("Invalid source folder/pak, does not exist");
                return;
            }

            // Check destination
            var destinationIsDirectory = System.IO.Path.GetExtension(destination) == string.Empty;

            // TODO - if source is folder, pak using options
            // TODO - if source is pak, unpack to destination
        }
    }
}