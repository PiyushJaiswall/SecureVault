using System;
using System.ComponentModel;
using System.IO;

namespace SecureVault.Models
{
    public class FileItem : INotifyPropertyChanged
    {
        private string _fileName;
        private string _filePath;
        private long _fileSize;
        private DateTime _createdDate;
        private DateTime _modifiedDate;
        private bool _isFolder;
        private string _fileType;

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public long FileSize
        {
            get => _fileSize;
            set
            {
                _fileSize = value;
                OnPropertyChanged(nameof(FileSize));
                OnPropertyChanged(nameof(FileSizeFormatted));
            }
        }

        public string FileSizeFormatted
        {
            get
            {
                if (_fileSize == 0) return "0 bytes";

                string[] sizes = { "bytes", "KB", "MB", "GB", "TB" };
                double len = _fileSize;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set
            {
                _createdDate = value;
                OnPropertyChanged(nameof(CreatedDate));
                OnPropertyChanged(nameof(CreatedDateFormatted));
            }
        }

        public string CreatedDateFormatted => _createdDate.ToString("yyyy-MM-dd HH:mm:ss");

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set
            {
                _modifiedDate = value;
                OnPropertyChanged(nameof(ModifiedDate));
                OnPropertyChanged(nameof(ModifiedDateFormatted));
            }
        }

        public string ModifiedDateFormatted => _modifiedDate.ToString("yyyy-MM-dd HH:mm:ss");

        public bool IsFolder
        {
            get => _isFolder;
            set
            {
                _isFolder = value;
                OnPropertyChanged(nameof(IsFolder));
                OnPropertyChanged(nameof(IsFile));
                OnPropertyChanged(nameof(FileType));
            }
        }

        public bool IsFile => !_isFolder;

        public string FileType
        {
            get
            {
                if (_isFolder) return "Folder";
                if (string.IsNullOrEmpty(_fileName)) return "File";

                var extension = Path.GetExtension(_fileName);
                return string.IsNullOrEmpty(extension) ? "File" : extension.ToUpper().TrimStart('.');
            }
            set
            {
                _fileType = value;
                OnPropertyChanged(nameof(FileType));
            }
        }

        public string VaultPath { get; set; } // Internal vault storage path

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FileItem()
        {
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        public FileItem(string fileName, string filePath, bool isFolder = false) : this()
        {
            FileName = fileName;
            FilePath = filePath;
            IsFolder = isFolder;
        }
    }
}
