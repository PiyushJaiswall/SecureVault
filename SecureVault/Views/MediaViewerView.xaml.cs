using SecureVault.ViewModels;
using System.Windows.Controls;

namespace SecureVault.Views
{
    public partial class MediaViewerView : UserControl
    {
        public MediaViewerView(MediaViewerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            // Pass the MediaElement to the ViewModel so it can be controlled
            viewModel.SetMediaPlayer(MediaPlayer);
        }
    }
}
