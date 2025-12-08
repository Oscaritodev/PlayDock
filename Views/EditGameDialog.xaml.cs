using System.Windows;
using NewLauncher.Models;

namespace NewLauncher.Views
{
    public partial class EditGameDialog : Window
    {
        public string GameTitle { get; private set; }
        public string GameDescription { get; private set; }

        public EditGameDialog(GameItem game)
        {
            InitializeComponent();
            TitleBox.Text = game.Title;
            DescBox.Text = game.Description;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            GameTitle = TitleBox.Text;
            GameDescription = DescBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
