namespace bg3_modders_multitool.Views
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        /// <summary>
        /// Displays the update window
        /// </summary>
        /// <param name="document">The document to display</param>
        /// <param name="changelog">Whether or not to display as a changelog</param>
        public Update(string document, bool changelog = false)
        {
            InitializeComponent();

            markdownViewer.Markdown = document;
            markdownViewer.Document.PagePadding = new Thickness(10);
            markdownViewer.Height = changelog ? 460 : 365;
            Title = changelog ? Properties.Resources.ChangeLogMenu : Properties.Resources.UpdatesFoundButton;
            buttonsBox.Visibility = changelog ? Visibility.Collapsed : Visibility.Visible;
            DataContext = this;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
