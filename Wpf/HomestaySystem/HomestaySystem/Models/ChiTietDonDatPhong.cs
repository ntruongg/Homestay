namespace HomestaySystem.Models
{
    /// <summary>
    /// Thực thể Chi tiết đơn đặt phòng.
    /// </summary>
    public class ChiTietDonDatPhong
    {
        public int MaChiTiet { get; set; }
        public int MaDon { get; set; }
        public int MaPhong { get; set; }
        public string TenPhong { get; set; } = string.Empty;
        public int SoLuongKhach { get; set; } = 2;
        public decimal DonGiaMoiDem { get; set; }
        public int SoDem { get; set; } = 1;
        public decimal PhuPhi { get; set; } = 0;
        public decimal GiamGia { get; set; } = 0;
        public decimal ThanhTien
        {
            get => (DonGiaMoiDem * SoDem) + PhuPhi - GiamGia;
            set { }
        }

        public string ThanhTienDinhDang
        {
            get => $"{ThanhTien:N0} đ";
            set { }
        }
    }
}
