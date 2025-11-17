using SecureVault.Views;
using System.Windows;

namespace SecureVault
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Navigate to password view on startup
            MainFrame.Navigate(new PasswordView());
        }

        public void NavigateToVault()
        {
            MainFrame.Navigate(new VaultView());
        }

        public void NavigateToLogin()
        {
            MainFrame.Navigate(new PasswordView());
        }

    }
}
