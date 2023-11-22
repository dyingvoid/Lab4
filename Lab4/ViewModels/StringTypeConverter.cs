using System;
using System.Globalization;
using System.Windows.Data;

namespace Lab4.ViewModels;

public class StringTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return "";

        var type = (Type) value;
        return type.Name;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var strType = (string) value;
        Type? type = null;

        try
        {
            type = Type.GetType($"System.{strType}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.StackTrace);
        }

        return type;
    }
}