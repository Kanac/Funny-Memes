using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace Comedian_Soundboard.Converters
{
    public class NameSplit : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var names = (value as string).Split(' ');
            StringBuilder formattedName = new StringBuilder();
            int count = 0;
            while (count < names.Length) {
                formattedName.Append(names[count++]);
                if (count != names.Length)
                    formattedName.Append("\n");
            }
            return formattedName.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
