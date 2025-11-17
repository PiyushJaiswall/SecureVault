using SecureVault.Models;
using System.Windows;

namespace SecureVault
{
    public partial class PropertiesWindow : Window
    {
        public PropertiesWindow(FileItem fileItem)
        {
            InitializeComponent();
            DataContext = fileItem;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
