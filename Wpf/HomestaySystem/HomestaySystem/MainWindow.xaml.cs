using System.Windows;
using HomestaySystem.ViewModels;

namespace HomestaySystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Hỗ trợ cả Dependency Injection (tiêm ManHinhChinhViewModel) và khởi tạo mặc định.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() : this(new ManHinhChinhViewModel())
        {
        }

        public MainWindow(ManHinhChinhViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}