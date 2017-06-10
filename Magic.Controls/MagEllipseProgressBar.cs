using System.Windows;
using System.Windows.Controls;

namespace Magic.Controls
{
    
    public class MagEllipseProgressBar : ProgressBar
    {
        static MagEllipseProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MagEllipseProgressBar), new FrameworkPropertyMetadata(typeof(MagEllipseProgressBar)));
        }
    }
}
