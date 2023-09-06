namespace bg3_modders_multitool.Views.Utilities
{
    using bg3_modders_multitool.ViewModels.Utilities;
    using System.Windows;

    /// <summary>
    /// Interaction logic for AtlasToolWindow.xaml
    /// </summary>
    public partial class AtlasToolWindow : Window
    {
        public AtlasToolWindow()
        {
            InitializeComponent();
            DataContext = new AtlasToolViewModel();
        }
    }
}
