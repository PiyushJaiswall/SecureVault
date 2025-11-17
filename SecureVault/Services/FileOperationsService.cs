using SecureVault.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace SecureVault.Services
{
    public class FileOperationsService
    {
        private readonly CryptoService _cryptoService;
        private readonly string _vaultBasePath;

        public MemoryStream DecryptFileToStream(string vaultPath, string fileName)
        {
            // fileName now includes the extension, so we add .enc to the full filename
            string encryptedFilePath = Path.Combine(_vaultBasePath, vaultPath, fileName + ".enc");

            if (!File.Exists(encryptedFilePath)) return null;

            var tempDecryptedStream = new MemoryStream();
            _cryptoService.DecryptFileToStream(encryptedFilePath, tempDecryptedStream);
            tempDecryptedStream.Position = 0;
            return tempDecryptedStream;
        }


        public FileOperationsService(CryptoService cryptoService, string vaultBasePath)
        {
            _cryptoService = cryptoService;
            _vaultBasePath = vaultBasePath;

            if (!Directory.Exists(_vaultBasePath))
            {
                Directory.CreateDirectory(_vaultBasePath);
            }
        }

        public void CreateFolder(string parentPath, string folderName)
        {
            var fullPath = Path.Combine(_vaultBasePath, parentPath, folderName);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        public void AddFileToVault(string sourceFilePath, string vaultPath, string fileName)
        {
            var vaultFilePath = Path.Combine(_vaultBasePath, vaultPath, fileName + ".enc");
            var directory = Path.GetDirectoryName(vaultFilePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _cryptoService.EncryptFile(sourceFilePath, vaultFilePath);
        }

        public void ExtractFileFromVault(string vaultPath, string fileName, string destinationPath)
        {
            var encryptedFilePath = Path.Combine(_vaultBasePath, vaultPath, fileName + ".enc");
            if (File.Exists(encryptedFilePath))
            {
                _cryptoService.DecryptFile(encryptedFilePath, destinationPath);
            }
        }

        public void DeleteFile(string vaultPath, string fileName)
        {
            var filePath = Path.Combine(_vaultBasePath, vaultPath, fileName + ".enc");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public void DeleteFolder(string vaultPath)
        {
            var folderPath = Path.Combine(_vaultBasePath, vaultPath);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }

        public void CopyFile(string sourceVaultPath, string sourceFileName, string destVaultPath, string destFileName)
        {
            var sourceFile = Path.Combine(_vaultBasePath, sourceVaultPath, sourceFileName + ".enc");
            var destFile = Path.Combine(_vaultBasePath, destVaultPath, destFileName + ".enc");

            var destDirectory = Path.GetDirectoryName(destFile);
            if (!Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, destFile, true);
            }
        }

        public void MoveFile(string sourceVaultPath, string sourceFileName, string destVaultPath, string destFileName)
        {
            CopyFile(sourceVaultPath, sourceFileName, destVaultPath, destFileName);
            DeleteFile(sourceVaultPath, sourceFileName);
        }

        public List<FileItem> GetFilesInDirectory(string vaultPath)
        {
            var files = new List<FileItem>();
            var fullPath = Path.Combine(_vaultBasePath, vaultPath);

            if (!Directory.Exists(fullPath))
                return files;

            // Add subdirectories
            foreach (var directory in Directory.GetDirectories(fullPath))
            {
                var dirInfo = new DirectoryInfo(directory);
                files.Add(new FileItem
                {
                    FileName = dirInfo.Name,
                    FilePath = Path.Combine(vaultPath, dirInfo.Name),
                    IsFolder = true,
                    CreatedDate = dirInfo.CreationTime,
                    ModifiedDate = dirInfo.LastWriteTime,
                    FileSize = 0,
                    VaultPath = Path.Combine(vaultPath, dirInfo.Name)
                });
            }

            // Add files (encrypted)
            foreach (var file in Directory.GetFiles(fullPath, "*.enc"))
            {
                var fileInfo = new FileInfo(file);
                var originalFileName = Path.GetFileNameWithoutExtension(fileInfo.Name);

                files.Add(new FileItem
                {
                    FileName = originalFileName,
                    FilePath = Path.Combine(vaultPath, originalFileName),
                    IsFolder = false,
                    CreatedDate = fileInfo.CreationTime,
                    ModifiedDate = fileInfo.LastWriteTime,
                    FileSize = fileInfo.Length,
                    VaultPath = Path.Combine(vaultPath, originalFileName)
                });
            }

            return files.OrderBy(f => f.IsFile).ThenBy(f => f.FileName).ToList();
        }

        public bool FileExists(string vaultPath, string fileName)
        {
            var filePath = Path.Combine(_vaultBasePath, vaultPath, fileName + ".enc");
            return File.Exists(filePath);
        }

        public bool FolderExists(string vaultPath)
        {
            var folderPath = Path.Combine(_vaultBasePath, vaultPath);
            return Directory.Exists(folderPath);
        }

        public byte[] GetFilePreview(string vaultPath, string fileName)
        {
            var encryptedFilePath = Path.Combine(_vaultBasePath, vaultPath, fileName + ".enc");
            if (File.Exists(encryptedFilePath))
            {
                var encryptedData = File.ReadAllBytes(encryptedFilePath);
                return _cryptoService.DecryptData(encryptedData);
            }
            return null;
        }
    }
}
