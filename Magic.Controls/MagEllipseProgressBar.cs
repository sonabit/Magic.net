using System.Windows;
using System.Windows.Controls;

namespace magic.Controls
{
    
    public class MagEllipseProgressBar : ProgressBar
    {
        static MagEllipseProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MagEllipseProgressBar), new FrameworkPropertyMetadata(typeof(MagEllipseProgressBar)));
        }
    }
}
