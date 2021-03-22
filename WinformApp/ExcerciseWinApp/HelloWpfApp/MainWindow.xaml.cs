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

namespace HelloWpfApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LblHello_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (LblHello.Foreground != Brushes.White)
            {
                LblHello.Foreground = Brushes.White;
                this.Background = Brushes.Pink; //글자 클릭하면 바탕색 바뀜

            }

            else
            {
                LblHello.Foreground = SystemColors.WindowTextBrush;
                this.Background = SystemColors.WindowBrush;
            }
        }
    }
}
