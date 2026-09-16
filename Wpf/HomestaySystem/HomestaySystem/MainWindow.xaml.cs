using System.Windows;
using HomestaySystem.ViewModels;

namespace HomestaySystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Cài đặt DataContext là ManHinhChinhViewModel điều phối toàn bộ phân hệ Quản trị.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ManHinhChinhViewModel();
        }
    }
}