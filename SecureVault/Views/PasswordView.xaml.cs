using SecureVault.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SecureVault.Views
{
    public partial class PasswordView : UserControl
    {
        private PasswordViewModel _viewModel;

        public PasswordView()
        {
            InitializeComponent();
            _viewModel = new PasswordViewModel();
            DataContext = _viewModel;
            _viewModel.AuthenticationResult += OnAuthenticationResult;

            // Focus password box on load
            Loaded += (s, e) => PasswordBox.Focus();
        }

        private void OnAuthenticationResult(object sender, bool success)
        {
            if (success)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.NavigateToVault();
            }
        }
    }
}
