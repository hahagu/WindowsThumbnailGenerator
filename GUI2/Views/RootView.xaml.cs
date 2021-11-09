using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI2.Views
{
    public partial class RootView : HandyControl.Controls.Window
    {
        public RootView()
        {
            InitializeComponent();

            Config_Tab.Content = new ConfigView();
        }
    }
}
