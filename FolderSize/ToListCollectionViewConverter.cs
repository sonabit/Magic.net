using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace FolderSize
{
    class ToListCollectionViewConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as IList;
            if (list != null)
            {
                var result = new ListCollectionView(list);
                result.LiveSortingProperties.Add("Value.TotalFileSize");
                result.SortDescriptions.Add(new SortDescription("Value.TotalFileSize", ListSortDirection.Descending));
                result.IsLiveSorting = true;
                return result;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
