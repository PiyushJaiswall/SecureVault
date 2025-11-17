using SecureVault.Services; // Add this using statement
using System;
using System.Windows;

namespace SecureVault
{
    public partial class App : Application
    {
        public static VaultService VaultService { get; private set; } // Add this property

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            VaultService = new VaultService(); // Initialize the shared service here

            // Set global exception handlers
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unexpected error occurred: {e.Exception.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Critical error: {((Exception)e.ExceptionObject).Message}", "Critical Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
