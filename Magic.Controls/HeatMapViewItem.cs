using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Magic.Controls
{ 
    public class HeatMapViewItem : Control
    {
        static HeatMapViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeatMapViewItem), new FrameworkPropertyMetadata(typeof(HeatMapViewItem)));
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(HeatMapViewItem), new PropertyMetadata(0D));
        
    }
}
