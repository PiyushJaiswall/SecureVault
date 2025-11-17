using SecureVault.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace SecureVault.Services
{
    public class VaultService
    {
        private CryptoService _cryptoService;
        private FileOperationsService _fileOperationsService;
        private readonly string _vaultPath;
        private readonly string _masterKeyPath; // --- ADD THIS LINE ---
        private bool _isUnlocked = false;

        public bool IsUnlocked => _isUnlocked;

        public VaultService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _vaultPath = Path.Combine(appDataPath, "SecureVault", "Vault_Data");
            _masterKeyPath = Path.Combine(_vaultPath, ".masterkey");

            if (!Directory.Exists(_vaultPath))
            {
                Directory.CreateDirectory(_vaultPath);
            }
        }
        // --- NEW METHOD ---
        public MemoryStream GetFileStream(string vaultPath, string fileName)
        {
            if (!_isUnlocked) return null;
            return _fileOperationsService.DecryptFileToStream(vaultPath, fileName);
        }

        public bool ChangePassword(string currentPassword, string newPassword)
        {
            // Verify the current password is correct
            var tempCrypto = new CryptoService(currentPassword);
            if (!tempCrypto.VerifyMasterKeyFile(_masterKeyPath))
            {
                return false; // Incorrect current password
            }

            // Create a new crypto service with the new password
            var newCryptoService = new CryptoService(newPassword);

            // Use the old crypto service to re-encrypt the master key with the new one
            _cryptoService.ChangeMasterKey(_masterKeyPath, newCryptoService);

            // Update the active crypto service to the new one
            _cryptoService = newCryptoService;
            _fileOperationsService = new FileOperationsService(_cryptoService, _vaultPath);

            return true;
        }

        // --- REPLACE THE OLD UnlockVault METHOD WITH THIS NEW ONE ---
        public bool UnlockVault(string password)
        {
            _cryptoService = new CryptoService(password);

            bool isNewVault = !File.Exists(_masterKeyPath);

            if (isNewVault)
            {
                // If it's a new vault, create the master key file with the new password
                _cryptoService.CreateMasterKeyFile(_masterKeyPath);
            }
            else
            {
                // If the vault exists, verify the password against the master key
                if (!_cryptoService.VerifyMasterKeyFile(_masterKeyPath))
                {
                    _isUnlocked = false;
                    return false; // Wrong password
                }
            }

            // If we get here, the password is correct (or it's a new vault)
            _fileOperationsService = new FileOperationsService(_cryptoService, _vaultPath);
            _isUnlocked = true;
            return true;
        }

        public void LockVault()
        {
            _cryptoService = null;
            _fileOperationsService = null;
            _isUnlocked = false;
        }

        public List<FileItem> GetFiles(string path = "")
        {
            if (!_isUnlocked) return new List<FileItem>();
            return _fileOperationsService.GetFilesInDirectory(path);
        }

        public void CreateFolder(string parentPath, string folderName)
        {
            if (!_isUnlocked) return;
            _fileOperationsService.CreateFolder(parentPath, folderName);
        }

        public void AddFile(string sourceFilePath, string vaultPath, string fileName)
        {
            if (!_isUnlocked) return;
            _fileOperationsService.AddFileToVault(sourceFilePath, vaultPath, fileName);
        }

        public void ExtractFile(string vaultPath, string fileName, string destinationPath)
        {
            if (!_isUnlocked) return;
            _fileOperationsService.ExtractFileFromVault(vaultPath, fileName, destinationPath);
        }

        public void DeleteItem(FileItem item)
        {
            if (!_isUnlocked) return;

            if (item.IsFolder)
            {
                _fileOperationsService.DeleteFolder(item.VaultPath);
            }
            else
            {
                var directory = Path.GetDirectoryName(item.VaultPath) ?? "";
                _fileOperationsService.DeleteFile(directory, Path.GetFileNameWithoutExtension(item.FileName));
            }
        }

        public void CopyItem(FileItem sourceItem, string destPath, string destName)
        {
            if (!_isUnlocked || sourceItem.IsFolder) return;

            var sourcePath = Path.GetDirectoryName(sourceItem.VaultPath) ?? "";
            var sourceFileName = Path.GetFileNameWithoutExtension(sourceItem.FileName);
            _fileOperationsService.CopyFile(sourcePath, sourceFileName, destPath, destName);
        }

        public void MoveItem(FileItem sourceItem, string destPath, string destName)
        {
            if (!_isUnlocked || sourceItem.IsFolder) return;

            var sourcePath = Path.GetDirectoryName(sourceItem.VaultPath) ?? "";
            var sourceFileName = Path.GetFileNameWithoutExtension(sourceItem.FileName);
            _fileOperationsService.MoveFile(sourcePath, sourceFileName, destPath, destName);
        }

        public bool ItemExists(string vaultPath, string name, bool isFolder)
        {
            if (!_isUnlocked) return false;

            return isFolder ?
                _fileOperationsService.FolderExists(Path.Combine(vaultPath, name)) :
                _fileOperationsService.FileExists(vaultPath, name);
        }
    }
}
