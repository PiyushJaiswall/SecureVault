using SecureVault.Helpers;
using SecureVault.Services;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SecureVault.ViewModels
{
    public class PasswordViewModel : INotifyPropertyChanged
    {
        private string _password;
        private string _errorMessage;
        private readonly VaultService _vaultService;

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                ErrorMessage = string.Empty;
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ICommand UnlockCommand { get; }
        public ICommand ExitCommand { get; }

        public event EventHandler<bool> AuthenticationResult;
        public event PropertyChangedEventHandler PropertyChanged;

        public PasswordViewModel()
        {
            _vaultService = App.VaultService; // Use the shared instance from App.xaml.cs
            UnlockCommand = new RelayCommand(ExecuteUnlock, CanExecuteUnlock);
            ExitCommand = new RelayCommand(ExecuteExit);
        }

        private bool CanExecuteUnlock(object parameter)
        {
            return !string.IsNullOrEmpty(Password);
        }

        private void ExecuteUnlock(object parameter)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Please enter a password.";
                    return;
                }

                if (_vaultService.UnlockVault(Password))
                {
                    AuthenticationResult?.Invoke(this, true);
                }
                else
                {
                    ErrorMessage = "Invalid password. Please try again.";
                    Password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        private void ExecuteExit(object parameter)
        {
            Application.Current.Shutdown();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
