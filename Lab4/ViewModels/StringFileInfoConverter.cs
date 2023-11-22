using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace Lab4.ViewModels
{
    internal class StringFileInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return "";

            FileInfo fileInfo = (FileInfo)value;
            return fileInfo.FullName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string filePath = (string)value;
            FileInfo? fileInfo = null;

            try
            {
                fileInfo = new FileInfo(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

            return fileInfo;
        }
    }
}
