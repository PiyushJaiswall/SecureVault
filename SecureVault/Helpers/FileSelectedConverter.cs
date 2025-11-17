using System;
using System.Globalization;
using System.Windows.Data;
using SecureVault.Models;

namespace SecureVault.Helpers
{
    public class FileSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is FileItem fileItem && !fileItem.IsFolder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
