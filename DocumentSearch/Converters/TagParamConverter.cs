using System;
using System.Globalization;
using System.Windows.Data;
using DocumentSearch.Models;

namespace DocumentSearch.Converters;

public class TagParamConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values != null && values.Length == 2 && values[0] is Document doc && values[1] is string tag)
        {
            return new DocumentTagParam { Document = doc, Tag = tag };
        }
        return null!;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
