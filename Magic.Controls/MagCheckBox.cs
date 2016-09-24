using System.Windows;

namespace Magic.Controls
{
    public class MagCheckBox : System.Windows.Controls.CheckBox
    {
        static MagCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MagCheckBox), new FrameworkPropertyMetadata(typeof(MagCheckBox)));
        }
    }
}