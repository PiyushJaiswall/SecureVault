using SecureVault.ViewModels;
using System.Windows.Controls;

namespace SecureVault.Views
{
    public partial class ChangePasswordView : UserControl
    {
        public ChangePasswordView(VaultViewModel parentViewModel)
        {
            InitializeComponent();
            DataContext = new ChangePasswordViewModel(parentViewModel);
        }
    }
}
