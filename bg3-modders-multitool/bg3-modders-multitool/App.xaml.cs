using System.Windows;
using System.Windows.Controls;

namespace bg3_mod_packer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            ToolTipService.ShowDurationProperty.OverrideMetadata(
                typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
        }
    }
}
