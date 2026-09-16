namespace HomestaySystem.Models
{
    /// <summary>
    /// Đối tượng DTO thống kê Doanh thu và Dòng tiền của sàn Homestay.
    /// Phản ánh rõ ràng: Tổng thu 100%, Hoa hồng sàn 15%, Quyết toán chủ nhà 85%.
    /// </summary>
    public class ThongKeDoanhThu
    {
        /// <summary>
        /// Tổng tiền khách đã thanh toán 100%
        /// </summary>
        public decimal TongThuTuKhach { get; set; }

        /// <summary>
        /// Doanh thu thực tế của Sàn thu về (15% hoa hồng)
        /// </summary>
        public decimal HoaHongSanHuong
        {
            get => TongThuTuKhach * 0.15m;
            set { }
        }

        /// <summary>
        /// Tiền thuộc về Chủ homestay (85%)
        /// </summary>
        public decimal TienTraChuHome
        {
            get => TongThuTuKhach * 0.85m;
            set { }
        }

        public int TongSoDon { get; set; }
        public int SoDonHoanThanh { get; set; }
        public int SoDonChuaQuyetToan { get; set; }
        public decimal SoTienChuaQuyetToan { get; set; }

        // ================= THUỘC TÍNH BỔ TRỢ GIAO DIỆN =================
        public string TongThuTuKhachDinhDang
        {
            get => $"{TongThuTuKhach:N0} đ";
            set { }
        }

        public string HoaHongSanHuongDinhDang
        {
            get => $"{HoaHongSanHuong:N0} đ";
            set { }
        }

        public string TienTraChuHomeDinhDang
        {
            get => $"{TienTraChuHome:N0} đ";
            set { }
        }

        public string SoTienChuaQuyetToanDinhDang
        {
            get => $"{SoTienChuaQuyetToan:N0} đ";
            set { }
        }
    }
}
