namespace bg3_modders_multitool.Views
{
    using System.Windows;
    using System.Windows.Documents;

    /// <summary>
    /// Interaction logic for Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        public Update(string document)
        {
            InitializeComponent();

            markdownViewer.Markdown = document;
            markdownViewer.Document.PagePadding = new Thickness(10);
        }
    }
}
