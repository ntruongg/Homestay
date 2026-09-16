using System;

namespace HomestaySystem.Models
{
    /// <summary>
    /// Thực thể Giao dịch thanh toán tiền phòng (Khách thanh toán 100% qua web).
    /// </summary>
    public class ThanhToan
    {
        public int MaThanhToan { get; set; }
        public int MaDon { get; set; }
        public string PhuongThucThanhToan { get; set; } = "VNPay"; // VNPay, MoMo, ZaloPay, TheNganHang
        public decimal SoTien { get; set; }
        public string MaGiaoDichWeb { get; set; } = string.Empty;
        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        /// <summary>
        /// Trạng thái thanh toán: "ThanhCong", "ThatBai", "HoanTien"
        /// </summary>
        public string TrangThaiThanhToan { get; set; } = "ThanhCong";

        public string SoTienDinhDang
        {
            get => $"{SoTien:N0} đ";
            set { }
        }
    }
}
