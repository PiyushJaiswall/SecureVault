using SecureVault.Helpers;
using SecureVault.Services;
using System.ComponentModel;
using System.Windows.Input;

namespace SecureVault.ViewModels
{
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private string _currentPassword;
        private string _newPassword;
        private string _confirmNewPassword;
        private string _statusMessage;
        private readonly VaultViewModel _parentViewModel;

        public string CurrentPassword { get => _currentPassword; set { _currentPassword = value; OnPropertyChanged(nameof(CurrentPassword)); } }
        public string NewPassword { get => _newPassword; set { _newPassword = value; OnPropertyChanged(nameof(NewPassword)); } }
        public string ConfirmNewPassword { get => _confirmNewPassword; set { _confirmNewPassword = value; OnPropertyChanged(nameof(ConfirmNewPassword)); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); } }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public ChangePasswordViewModel(VaultViewModel parentViewModel)
        {
            _parentViewModel = parentViewModel;
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
        }

        private bool CanExecuteSave(object obj)
        {
            return !string.IsNullOrEmpty(CurrentPassword) && !string.IsNullOrEmpty(NewPassword) && !string.IsNullOrEmpty(ConfirmNewPassword);
        }

        private void ExecuteSave(object obj)
        {
            if (NewPassword != ConfirmNewPassword)
            {
                StatusMessage = "New passwords do not match.";
                return;
            }

            if (_parentViewModel.ChangePassword(CurrentPassword, NewPassword))
            {
                StatusMessage = "Password changed successfully!";
                _parentViewModel.CloseChangePasswordView();
            }
            else
            {
                StatusMessage = "Incorrect current password.";
            }
        }

        private void ExecuteCancel(object obj)
        {
            _parentViewModel.CloseChangePasswordView();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
