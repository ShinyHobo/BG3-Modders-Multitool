using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using avalonia_multitool.Services;
using avalonia_multitool.ViewModels;
using avalonia_multitool.Views;
using CommandLine;

namespace avalonia_multitool;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop!.Startup += (sender, args) =>
            {
                if (args.Args.Length > 0)
                {
                    desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                    var wnd = new MainWindow();
                    desktop.MainWindow = new Window { DataContext = wnd };

#if _WINDOWS
                    if (!Cli.AttachConsole(-1))
                    {
                        // for debugging
                        Cli.AllocConsole();
                    }
#else
                    // TODO - attach to terminal?
#endif

                    // TODO - replace with static class to keep track of cli properites
                    //App.Current.Properties["console_app"] = true;

                    Parser.Default.ParseArguments<Cli>(args.Args).WithParsedAsync(Cli.Run).Wait();

                    desktop.Shutdown();
                }
                else
                {
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainViewModel()
                    };
                }
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
