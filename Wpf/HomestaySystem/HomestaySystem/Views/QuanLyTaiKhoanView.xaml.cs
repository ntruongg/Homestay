using System.Windows.Controls;

namespace HomestaySystem.Views
{
    /// <summary>
    /// Interaction logic for QuanLyTaiKhoanView.xaml
    /// </summary>
    public partial class QuanLyTaiKhoanView : UserControl
    {
        public QuanLyTaiKhoanView()
        {
            InitializeComponent();
            Loaded += (s, e) => svChiTiet?.ScrollToTop();
            DataContextChanged += (s, e) => svChiTiet?.ScrollToTop();
            if (svChiTiet != null)
            {
                svChiTiet.RequestBringIntoView += (s, e) => e.Handled = true;
            }
            if (dgNguoiDung != null)
            {
                dgNguoiDung.SelectionChanged += (s, e) => svChiTiet?.ScrollToTop();
            }
        }
    }
}
