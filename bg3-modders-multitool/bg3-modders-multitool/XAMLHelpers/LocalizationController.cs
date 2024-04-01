using bg3_modders_multitool;
using System.Windows;
using System;
using bg3_modders_multitool.Views;
using CommandLine;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using static System.Windows.Forms.Design.AxImporter;

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
            App app = new App();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var wnd = new bg3_modders_multitool.ViewModels.MainWindow();
            app.MainWindow = new Window { DataContext = wnd };

            AttachConsole(-1);

            App.Current.Properties["console_app"] = true;

            Parser.Default.ParseArguments<Cli>(args).WithParsedAsync(Cli.Run).Wait();

            App.Current.Shutdown();
        }
        else
        {
            App app = new App();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            MainWindow wnd = new MainWindow();
            wnd.Closed += Wnd_Closed;
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
    private class Cli // TODO - set up translation resources for these
    {
        /// <summary>
        /// The source folder/file
        /// </summary>
        [Option('s', "source", HelpText = "Input folder/file")]
        public string Source { get; set; }

        /// <summary>
        /// The destination folder/file
        /// </summary>
        [Option('d', "destination", HelpText = "Output folder/file")]
        public string Destination { get; set; }

        /// <summary>
        /// The compression level to use (0-4)
        /// </summary>
        [Option('c', "compression", HelpText = "0: None\r\n1: LZ4\r\n2: LZ4 HC\r\n3: Zlib Fast\r\n4: Zlib Optimal")]
        public int Compression { get; set; }

        [Option('o', "out", HelpText = "Writes console output to the given file instead of the console window")]
        public string WriteToFile { get; set; }

        [Option('a', "append", HelpText = "Appends to the out file rather than overwriting it")]
        public bool AppendToFile { get; set; }

        public string GetUsage()
        {
            return "Read wiki for usage instructions...";
        }

        /// <summary>
        /// Runs the multitool cli commands
        /// </summary>
        /// <param name="options">The cli options</param>
        /// <returns>The task</returns>
        public static async Task Run(Cli options)
        {
            var source = options.Source == null ? null : Path.GetFullPath(options.Source);
            var destination = options.Destination == null ? null : Path.GetFullPath(options.Destination);
            var compression = bg3_modders_multitool.ViewModels.MainWindow.AvailableCompressionTypes.FirstOrDefault(c => c.Id == options.Compression);
            
            var writeToFile = options.WriteToFile == null ? null : Path.GetFullPath(options.WriteToFile);

            using (var writer = writeToFile != null ? new System.IO.StreamWriter(writeToFile, options.AppendToFile) : null)
            {
                if(options.WriteToFile != null)
                {
                    Console.SetOut(writer);
                }

                if (source != null && destination == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Destination required!");
                    Console.ResetColor();
                    return;
                }

                if (source == null && destination != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Source required!");
                    Console.ResetColor();
                    return;
                }

                // Check source (must exist)
                var sourceIsDirectory = System.IO.Path.GetExtension(source) == string.Empty;
                if ((sourceIsDirectory && !Directory.Exists(source)) || (!sourceIsDirectory && !File.Exists(source)))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid source folder/pak, does not exist");
                    Console.ResetColor();
                    return;
                }

                // Check destination
                var destinationExtension = System.IO.Path.GetExtension(destination);
                var destinationIsDirectory = destinationExtension == string.Empty;

                // Set global config
                App.Current.Properties["cli_source"] = source;
                App.Current.Properties["cli_destination"] = destination;
                App.Current.Properties["cli_compression"] = compression.Id;
                App.Current.Properties["cli_zip"] = destinationExtension == ".zip";

                if (sourceIsDirectory && !destinationIsDirectory)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Packing mod...");
                    Console.ResetColor();

                    DataObject data = new DataObject(DataFormats.FileDrop, new string[] { source });
                    var dadVm = new bg3_modders_multitool.ViewModels.DragAndDropBox();
                    await dadVm.ProcessDrop(data);
                }
                else if (destinationIsDirectory && !sourceIsDirectory)
                {
                    var sourceExtension = System.IO.Path.GetExtension(source);
                    if (sourceExtension == ".pak")
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Unpacking mod...");
                        Console.ResetColor();

                        var vm = new bg3_modders_multitool.ViewModels.MainWindow();
                        await vm.Unpacker.UnpackPakFiles(new List<string> { source }, false);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid source extension. File must be a .pak!");
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Invalid operation; source and destination cannot both be {(sourceIsDirectory && destinationIsDirectory ? "directories" : "files")}!");
                    Console.ResetColor();
                }
            }
        }
    }
}