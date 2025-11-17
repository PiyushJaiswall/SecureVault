using SecureVault.Helpers;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SecureVault.ViewModels
{
    public class MediaViewerViewModel : INotifyPropertyChanged
    {
        private VaultViewModel _parentViewModel;
        private MediaElement _mediaPlayer;

        public ImageSource ImageSource { get; set; }
        public Uri MediaUri { get; set; }
        public bool IsImageVisible { get; set; }
        public bool IsMediaVisible { get; set; }

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand CloseCommand { get; }

        public MediaViewerViewModel(VaultViewModel parent, MemoryStream stream, string fileExtension)
        {
            _parentViewModel = parent;

            // Commands
            PlayCommand = new RelayCommand(p => _mediaPlayer?.Play());
            PauseCommand = new RelayCommand(p => _mediaPlayer?.Pause());
            StopCommand = new RelayCommand(p => _mediaPlayer?.Stop());
            CloseCommand = new RelayCommand(p => Close());

            // Determine if it's an image or media and load it
            var ext = fileExtension.ToLower();
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".gif")
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                ImageSource = bitmap;
                IsImageVisible = true;
            }
            else if (ext == ".mp4" || ext == ".wmv" || ext == ".mp3" || ext == ".wav")
            {
                // To play from memory, we must save it to a temp file that we clean up later
                string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ext);
                File.WriteAllBytes(tempFilePath, stream.ToArray());
                MediaUri = new Uri(tempFilePath);
                IsMediaVisible = true;
            }
        }

        public void SetMediaPlayer(MediaElement player)
        {
            _mediaPlayer = player;
            if (IsMediaVisible)
            {
                _mediaPlayer.Play(); // Autoplay media
            }
        }

        private void Close()
        {
            // Stop media and clean up the temporary file if one was created
            if (_mediaPlayer != null)
            {
                _mediaPlayer.Stop();
                _mediaPlayer.Source = null;
            }

            if (MediaUri != null && File.Exists(MediaUri.LocalPath))
            {
                File.Delete(MediaUri.LocalPath);
            }

            _parentViewModel.CloseMediaViewer();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
