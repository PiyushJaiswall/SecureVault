using Microsoft.Win32;
using SecureVault.Helpers;
using SecureVault.Models;
using SecureVault.Services;
using SecureVault.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SecureVault.ViewModels
{
    public class VaultViewModel : INotifyPropertyChanged
    {
        private readonly VaultService _vaultService;
        private ObservableCollection<FileItem> _files;
        private FileItem _selectedFile;
        private string _currentPath;
        private string _statusMessage;

        public ObservableCollection<FileItem> Files
        {
            get => _files;
            set
            {
                _files = value;
                OnPropertyChanged(nameof(Files));
            }
        }

        public FileItem SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged(nameof(SelectedFile));
            }
        }

        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                _currentPath = value;
                OnPropertyChanged(nameof(CurrentPath));
                OnPropertyChanged(nameof(CurrentPathDisplay));
            }
        }

        public string CurrentPathDisplay => string.IsNullOrEmpty(CurrentPath) ? "Vault Root" : CurrentPath.Replace("\\", " > ");


        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        // Commands
        public ICommand DoubleClickCommand { get; }
        public ICommand ShowChangePasswordCommand { get; }
        public ICommand NavigateUpCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand AddFileCommand { get; }
        public ICommand CreateFolderCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand PropertiesCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand LockVaultCommand { get; }
        public ICommand ExtractCommand { get; }

        // Context menu commands
        public ICommand ContextDeleteCommand { get; }
        public ICommand ContextCopyCommand { get; }
        public ICommand ContextCutCommand { get; }
        public ICommand ContextPropertiesCommand { get; }

        private FileItem _clipboardItem;
        private bool _isCut;

        public event EventHandler VaultLocked;
        public event PropertyChangedEventHandler PropertyChanged;

        private object _modalContent;
        public object ModalContent
        {
            get => _modalContent;
            set { _modalContent = value; OnPropertyChanged(nameof(ModalContent)); }
        }

        private void ExecuteShowChangePassword(object obj)
        {
            ModalContent = new ChangePasswordView(this);
        }

        public void CloseChangePasswordView()
        {
            ModalContent = null;
        }

        public bool ChangePassword(string currentPassword, string newPassword)
        {
            return _vaultService.ChangePassword(currentPassword, newPassword);
        }

        public VaultViewModel(VaultService vaultService)
        {
            _vaultService = vaultService;
            Files = new ObservableCollection<FileItem>();
            CurrentPath = "";

            // Initialize commands with DEBUG MESSAGES
            DoubleClickCommand = new RelayCommand(ExecuteOpen, p => SelectedFile != null);
            OpenCommand = new RelayCommand(ExecuteOpen, p => SelectedFile != null);


            NavigateUpCommand = new RelayCommand(ExecuteNavigateUp, CanExecuteNavigateUp);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            AddFileCommand = new RelayCommand(ExecuteAddFile);
            CreateFolderCommand = new RelayCommand(ExecuteCreateFolder);
            DeleteCommand = new RelayCommand(ExecuteDelete, CanExecuteFileOperation);
            CopyCommand = new RelayCommand(ExecuteCopy, CanExecuteFileOperation);
            CutCommand = new RelayCommand(ExecuteCut, CanExecuteFileOperation);
            PasteCommand = new RelayCommand(ExecutePaste, CanExecutePaste);
            PropertiesCommand = new RelayCommand(ExecuteProperties, CanExecuteFileOperation);
            LockVaultCommand = new RelayCommand(ExecuteLockVault);
            ExtractCommand = new RelayCommand(ExecuteExtract, CanExecuteExtract);
            ShowChangePasswordCommand = new RelayCommand(ExecuteShowChangePassword);

            // Context menu commands
            ContextDeleteCommand = new RelayCommand(ExecuteContextDelete);
            ContextCopyCommand = new RelayCommand(ExecuteContextCopy);
            ContextCutCommand = new RelayCommand(ExecuteContextCut);
            ContextPropertiesCommand = new RelayCommand(ExecuteContextProperties);

            LoadFiles();
        }


        private void LoadFiles()
        {
            try
            {
                var files = _vaultService.GetFiles(CurrentPath);
                Files.Clear();
                foreach (var file in files)
                {
                    Files.Add(file);
                }
                StatusMessage = $"Loaded {files.Count} items";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading files: {ex.Message}";
            }
        }

        private bool CanExecuteNavigateUp(object parameter)
        {
            return !string.IsNullOrEmpty(CurrentPath);
        }

        private void ExecuteNavigateUp(object parameter)
        {
            var parentPath = Path.GetDirectoryName(CurrentPath);
            CurrentPath = parentPath ?? "";
            LoadFiles();
        }

        private void ExecuteRefresh(object obj)
        {
            Files.Clear();
            var filesInVault = _vaultService.GetFiles(CurrentPath);
            foreach (var file in filesInVault)
            {
                Files.Add(file);
            }
            StatusMessage = $"Vault refreshed. Found {Files.Count} items.";
        }

        private void ExecuteAddFile(object parameter)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select files to add to vault",
                Multiselect = true,
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    try
                    {
                        var fileName = Path.GetFileName(filePath);
                        _vaultService.AddFile(filePath, CurrentPath, fileName);
                        StatusMessage = $"Added {fileName} to vault";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding file {Path.GetFileName(filePath)}: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                LoadFiles();
            }
        }


        private void ExecuteOpenFile(object obj)
        {
            if (SelectedFile == null || SelectedFile.IsFolder) return;

            var fileExt = Path.GetExtension(SelectedFile.FileName).ToLower();
            var supportedExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".mp4", ".wmv", ".mp3", ".wav" };

            if (Array.Exists(supportedExtensions, e => e == fileExt))
            {
                // Use CurrentPath which is correct even when empty (for root vault files)
                var directory = CurrentPath;
                var fileNameWithExt = SelectedFile.FileName; // Use the full filename with extension

                using (var stream = _vaultService.GetFileStream(directory, fileNameWithExt))
                {
                    if (stream != null)
                    {
                        var mediaViewModel = new MediaViewerViewModel(this, stream, fileExt);
                        ModalContent = new MediaViewerView(mediaViewModel);
                        StatusMessage = $"Opened file: {SelectedFile.FileName}";
                    }
                    else
                    {
                        StatusMessage = "Error: Could not decrypt file. Check file integrity.";
                    }
                }
            }
            else
            {
                StatusMessage = "File type not supported for in-app viewing. Please use 'Extract'.";
            }
        }

        public void CloseMediaViewer()
        {
            ModalContent = null;
            RefreshCommand.Execute(null);
        }

        private void ExecuteCreateFolder(object parameter)
        {
            var folderName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter folder name:", "Create Folder", "New Folder");

            if (!string.IsNullOrWhiteSpace(folderName))
            {
                try
                {
                    _vaultService.CreateFolder(CurrentPath, folderName);
                    LoadFiles();
                    StatusMessage = $"Created folder: {folderName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating folder: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteFileOperation(object parameter)
        {
            return SelectedFile != null;
        }

        private bool CanExecuteExtract(object parameter)
        {
            return SelectedFile != null && !SelectedFile.IsFolder; // Changed from IsFile to !IsFolder
        }

        private void ExecuteDelete(object parameter)
        {
            if (SelectedFile == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedFile.FileName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _vaultService.DeleteItem(SelectedFile);
                    LoadFiles();
                    StatusMessage = $"Deleted: {SelectedFile.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting item: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExecuteCopy(object parameter)
        {
            if (SelectedFile != null)
            {
                _clipboardItem = SelectedFile;
                _isCut = false;
                StatusMessage = $"Copied: {SelectedFile.FileName}";
            }
        }

        private void ExecuteCut(object parameter)
        {
            if (SelectedFile != null)
            {
                _clipboardItem = SelectedFile;
                _isCut = true;
                StatusMessage = $"Cut: {SelectedFile.FileName}";
            }
        }

        private bool CanExecutePaste(object parameter)
        {
            return _clipboardItem != null;
        }

        private void ExecutePaste(object parameter)
        {
            if (_clipboardItem == null) return;

            try
            {
                var newName = _clipboardItem.FileName;
                var counter = 1;

                // Handle name conflicts
                while (_vaultService.ItemExists(CurrentPath, newName, _clipboardItem.IsFolder))
                {
                    var extension = Path.GetExtension(_clipboardItem.FileName);
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(_clipboardItem.FileName);
                    newName = $"{nameWithoutExt} - Copy ({counter}){extension}";
                    counter++;
                }

                if (_isCut)
                {
                    _vaultService.MoveItem(_clipboardItem, CurrentPath, newName);
                    StatusMessage = $"Moved: {newName}";
                }
                else
                {
                    _vaultService.CopyItem(_clipboardItem, CurrentPath, newName);
                    StatusMessage = $"Copied: {newName}";
                }

                LoadFiles();
                _clipboardItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error pasting item: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteProperties(object parameter)
        {
            if (SelectedFile != null)
            {
                var propertiesWindow = new PropertiesWindow(SelectedFile);
                propertiesWindow.ShowDialog();
            }
        }

        private void ExecuteOpen(object parameter)
        {
            if (SelectedFile == null) return;

            if (SelectedFile.IsFolder)
            {
                // Navigate into the selected folder
                CurrentPath = Path.Combine(CurrentPath, SelectedFile.FileName);
                LoadFiles();
            }
            else
            {
                // For files, attempt to open them in the media viewer
                ExecuteOpenFile(parameter);
            }
        }

        private void ExecuteExtract(object parameter)
        {
            if (SelectedFile == null || SelectedFile.IsFolder) return;

            var saveFileDialog = new SaveFileDialog
            {
                Title = "Extract file to...",
                FileName = SelectedFile.FileName,
                Filter = "All Files (*.*)|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _vaultService.ExtractFile(Path.GetDirectoryName(SelectedFile.VaultPath) ?? "",
                        Path.GetFileNameWithoutExtension(SelectedFile.FileName), saveFileDialog.FileName);
                    StatusMessage = $"Extracted: {SelectedFile.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error extracting file: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void NavigateTo(string folderName)
        {
            CurrentPath = Path.Combine(CurrentPath, folderName);
            RefreshCommand.Execute(null);
        }

        private void ExecuteLockVault(object parameter)
        {
            var result = MessageBox.Show("Lock vault and return to login screen?", "Lock Vault",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _vaultService.LockVault();
                VaultLocked?.Invoke(this, EventArgs.Empty);
            }
        }

        // Context menu command implementations
        private void ExecuteContextDelete(object parameter)
        {
            if (parameter is FileItem item)
            {
                SelectedFile = item;
                ExecuteDelete(parameter);
            }
        }

        private void ExecuteContextCopy(object parameter)
        {
            if (parameter is FileItem item)
            {
                SelectedFile = item;
                ExecuteCopy(parameter);
            }
        }

        private void ExecuteContextCut(object parameter)
        {
            if (parameter is FileItem item)
            {
                SelectedFile = item;
                ExecuteCut(parameter);
            }
        }

        private void ExecuteContextProperties(object parameter)
        {
            if (parameter is FileItem item)
            {
                SelectedFile = item;
                ExecuteProperties(parameter);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
