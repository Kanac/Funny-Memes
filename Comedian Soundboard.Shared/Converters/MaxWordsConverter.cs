using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace Comedian_Soundboard.Converters
{
    public class MaxWordsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string val = value.ToString();
            int maxWords = System.Convert.ToInt32(parameter);
            StringBuilder sb = new StringBuilder();
            foreach (var chara in val)
            {
                if (chara == ' ')
                {
                    --maxWords;
                    if (maxWords == 0)
                        break;
                }

                sb.Append(chara);
            }

            return sb.ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
