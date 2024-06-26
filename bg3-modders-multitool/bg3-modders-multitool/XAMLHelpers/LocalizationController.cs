﻿using bg3_modders_multitool;
using System.Windows;
using System;
using bg3_modders_multitool.Views;
using CommandLine;
using System.Threading.Tasks;
using Alphaleonis.Win32.Filesystem;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Globalization;

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
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            App app = new App();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var wnd = new bg3_modders_multitool.ViewModels.MainWindow();
            app.MainWindow = new MainWindow { DataContext = wnd };

            if(!AttachConsole(-1))
            {
                // for debugging
                AllocConsole();
            }

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
    static extern bool AllocConsole();

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

        [Option('a', "append", HelpText = "Appends to the out file rather than overwriting it; requires --out")]
        public bool AppendToFile { get; set; }

        [Option("delete-index", HelpText = "Deletes the index; will ask for confirmation")]
        public bool DeleteIndex { get; set; }

        [Option("delete-index-force", HelpText = "Deletes the index without confirmation")]
        public bool ForceDeleteIndex { get; set; }

        [Option("create-index", HelpText = "Generates a new index; will ask for confirmation")]
        public bool CreateIndex { get; set; }

        [Option("search-index", HelpText = "Searches the index for the given string; requires --index-results")]
        public string SearchIndex { get; set; }

        [Option("search-filter", HelpText = "Filters the search results by the comma delimited extension list provided (ie. \"lsf,loca,png\"); requires --search-index")]
        public string FilterIndex { get; set; }

        [Option("index-results", HelpText = "The file to print the index results to")]
        public string IndexResultsFile { get; set; }

        [Option("fast-search", HelpText = "Disables the implicit leading wildcard for searches, drastically speeding them up, but may miss results if used incorrectly")]
        public bool FastSearch { get; set; }

        [Option('v', "version", HelpText = "Sets the version in the meta.lsx file")]
        public string Version { get; set; }

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
            var version = options.Version;

            
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
                    Console.WriteLine("Destination required! Check --help");
                    Console.ResetColor();
                    return;
                }

                if (source == null && destination != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Source required! Check --help");
                    Console.ResetColor();
                    return;
                }

                // Check source (must exist)
                var sourceIsDirectory = System.IO.Path.GetExtension(source) == string.Empty;
                if (source != null && ((sourceIsDirectory && !Directory.Exists(source)) || (!sourceIsDirectory && !File.Exists(source))))
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

                if (source != null && destination != null)
                {
                    if (sourceIsDirectory && !destinationIsDirectory)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Packing mod...");
                        Console.ResetColor();

                        DataObject data = new DataObject(DataFormats.FileDrop, new string[] { source });
                        var dadVm = new bg3_modders_multitool.ViewModels.DragAndDropBox();

                        if(version != null)
                        {
                            var versionSegs = version.Split('.');
                            if(versionSegs.Length == 4)
                            {
                                dadVm.CanRebuild = Visibility.Visible;
                                int.TryParse(versionSegs[0], out int major);
                                int.TryParse(versionSegs[1], out int minor);
                                int.TryParse(versionSegs[2], out int revision);
                                int.TryParse(versionSegs[3], out int build);

                                dadVm.Major = major;
                                dadVm.Minor = minor;
                                dadVm.Revision = revision;
                                dadVm.Build = build;
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Invalid version format; use MAJOR.MINOR.REVISION.BUILD. Defaulting to detected meta.lsx version...");
                                Console.ResetColor();
                            }
                        }

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
                else
                {
                    if(options.DeleteIndex || options.ForceDeleteIndex)
                    {
                        if(!options.ForceDeleteIndex)
                        {
                            Console.Write("This will delete your index; this is irreversable. Are you sure? [y/n]:");
                            var response = Console.ReadKey(false).Key;
                            Console.WriteLine(string.Empty);
                            if (response != ConsoleKey.Y)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("Operation cancelled.");
                                Console.ResetColor();
                                return;
                            }
                        }

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Deleting index...");
                        Console.ResetColor();

                        var vm = new bg3_modders_multitool.ViewModels.MainWindow()
                        {
                            SearchResults = new bg3_modders_multitool.ViewModels.SearchResults()
                        };

                        vm.SearchResults.IndexHelper.DeleteIndex();

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Index deleted; run --create-index to regenerate");
                        Console.ResetColor();
                    }

                    if(options.CreateIndex)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Indexing; this could take a while...");
                        Console.ResetColor();

                        var vm = new bg3_modders_multitool.ViewModels.MainWindow() {
                            SearchResults = new bg3_modders_multitool.ViewModels.SearchResults()
                        };

                        await vm.SearchResults.IndexHelper.IndexDirectly();
                    }
                    
                    if(options.SearchIndex != null)
                    {
                        if(options.IndexResultsFile == null)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Index results file location required! Check --help");
                            Console.ResetColor();
                            return;
                        }

                        // sanitize file formats
                        var fileTypes = options.FilterIndex?.Split(',').Select(f => $".{f.Replace(".", string.Empty)}").ToList();

                        var vm = new bg3_modders_multitool.ViewModels.MainWindow()
                        {
                            SearchResults = new bg3_modders_multitool.ViewModels.SearchResults()
                        };

                        using (var resultsWriter = new System.IO.StreamWriter(options.IndexResultsFile))
                        {
                            var matches = await vm.SearchResults.IndexHelper.SearchFiles(options.SearchIndex, true, fileTypes, !options.FastSearch);
                            // TODO - do something with FilteredMatches?
                             foreach (var match in matches.Matches)
                            {
                                resultsWriter.WriteLine(match);
                            }
                        }
                    }
                }
            }
        }
    }
}