using SecureVault.Services;
using SecureVault.ViewModels;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input; // ADD THIS LINE

namespace SecureVault.Views
{
    public partial class VaultView : UserControl
    {
        private VaultViewModel _viewModel;

        public VaultView()
        {
            InitializeComponent();

            var vaultService = App.VaultService;
            _viewModel = new VaultViewModel(vaultService);
            DataContext = _viewModel;

            _viewModel.VaultLocked += OnVaultLocked;
        }

        private void OnVaultLocked(object sender, System.EventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            mainWindow?.NavigateToLogin();
        }

        // ADD THIS NEW METHOD FOR DOUBLE-CLICK
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Execute the OpenCommand when double-clicking on a list item
            if (_viewModel?.SelectedFile != null)
            {
                if (_viewModel.OpenCommand.CanExecute(null))
                {
                    _viewModel.OpenCommand.Execute(null);
                }
            }
        }
    }

    // Helper converter for folder/file icons
    public class BoolToIconConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFolder)
            {
                return isFolder ? "📁" : "📄";
            }
            return "📄";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
