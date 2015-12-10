using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Magic.Controls.Converter
{
    public class ReadableFilesizeConverter : IValueConverter
    {
        static readonly string[] Sizes = { "B", "KB", "MB", "GB", "TB" };

        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double size = double.NaN;
            if (value is double)
                size = (double) value;
            else if (value is string)
            {
                if (!double.TryParse((string) value, out size))
                    return value;
            }
            else 
            {
                try
                {
                    size = System.Convert.ToDouble(value);
                }
                catch
                {
                    return value;
                }
            }

            if (double.IsNaN(size)) return value;

            int order = 0;
            while (size >= 1024 && order + 1 < Sizes.Length)
            {
                order++;
                size = size / 1024;
            }

            return string.Format("{0:0.##} {1}", size, Sizes[order]);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
