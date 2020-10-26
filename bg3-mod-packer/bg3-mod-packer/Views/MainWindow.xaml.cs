using System.Windows;
using System.Configuration;

namespace bg3_mod_packer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            divineLocation.Text = Properties.Settings.Default.divineExe;
            divineLocation.ToolTip = divineLocation.Text;

        }

        private void DivineSelect_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = fileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    var file = fileDialog.FileName;
                    divineLocation.Text = file;
                    divineLocation.ToolTip = divineLocation.Text;
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    divineLocation.Text = null;
                    divineLocation.ToolTip = divineLocation.Text;
                    break;
            }
            Properties.Settings.Default.divineExe = divineLocation.Text;
            Properties.Settings.Default.Save();
        }
        }
    }
}
