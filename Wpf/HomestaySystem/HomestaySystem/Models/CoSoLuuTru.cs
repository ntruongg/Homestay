using System;
using System.Collections.Generic;
using HomestaySystem.ViewModels;

namespace HomestaySystem.Models
{
    /// <summary>
    /// Thực thể Cơ sở lưu trú (Homestay) được đăng ký bởi Chủ home trên sàn.
    /// Bao gồm hồ sơ pháp lý (Giấy phép kinh doanh, PCCC, An ninh trật tự) để Admin kiểm duyệt.
    /// </summary>
    public class CoSoLuuTru : ViewModelBase
    {
        private int _maCoSo;
        public int MaCoSo
        {
            get => _maCoSo;
            set
            {
                if (SetProperty(ref _maCoSo, value))
                {
                    OnPropertyChanged(nameof(MaCoSoHienThi));
                    OnPropertyChanged(nameof(MaHoSoHienThi));
                }
            }
        }

        private string _tenCoSo = string.Empty;
        public string TenCoSo
        {
            get => _tenCoSo;
            set => SetProperty(ref _tenCoSo, value);
        }

        private string _dienThoai = string.Empty;
        public string DienThoai
        {
            get => _dienThoai;
            set => SetProperty(ref _dienThoai, value);
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _diaChi = string.Empty;
        public string DiaChi
        {
            get => _diaChi;
            set => SetProperty(ref _diaChi, value);
        }

        private string _phuongXa = string.Empty;
        public string PhuongXa
        {
            get => _phuongXa;
            set => SetProperty(ref _phuongXa, value);
        }

        private string _tinhThanh = string.Empty;
        public string TinhThanh
        {
            get => _tinhThanh;
            set
            {
                if (SetProperty(ref _tinhThanh, value))
                {
                    OnPropertyChanged(nameof(KhuVuc));
                }
            }
        }

        private string _moTa = string.Empty;
        public string MoTa
        {
            get => _moTa;
            set => SetProperty(ref _moTa, value);
        }

        private string _chinhSach = string.Empty;
        public string ChinhSach
        {
            get => _chinhSach;
            set => SetProperty(ref _chinhSach, value);
        }

        private string _chinhSachHuyPhong = "Miễn phí hủy trước 48 giờ.";
        public string ChinhSachHuyPhong
        {
            get => _chinhSachHuyPhong;
            set => SetProperty(ref _chinhSachHuyPhong, value);
        }

        private bool _trangThaiHoatDong = true;
        public bool TrangThaiHoatDong
        {
            get => _trangThaiHoatDong;
            set => SetProperty(ref _trangThaiHoatDong, value);
        }

        // ================= THÔNG TIN CHỦ SỞ HỮU =================
        private int _maChuHome;
        public int MaChuHome
        {
            get => _maChuHome;
            set => SetProperty(ref _maChuHome, value);
        }

        private string _tenChuHome = string.Empty;
        public string TenChuHome
        {
            get => _tenChuHome;
            set
            {
                if (SetProperty(ref _tenChuHome, value))
                {
                    OnPropertyChanged(nameof(ChuTaiKhoanHienThi));
                }
            }
        }

        private string _soDienThoaiChuHome = string.Empty;
        public string SoDienThoaiChuHome
        {
            get => _soDienThoaiChuHome;
            set => SetProperty(ref _soDienThoaiChuHome, value);
        }

        private string _emailChuHome = string.Empty;
        public string EmailChuHome
        {
            get => _emailChuHome;
            set => SetProperty(ref _emailChuHome, value);
        }

        private string _soCCCDChuHome = string.Empty;
        public string SoCCCDChuHome
        {
            get => _soCCCDChuHome;
            set
            {
                if (SetProperty(ref _soCCCDChuHome, value))
                {
                    OnPropertyChanged(nameof(CCCDHienThi));
                }
            }
        }

        private string _tenNganHangChuHome = string.Empty;
        public string TenNganHangChuHome
        {
            get => _tenNganHangChuHome;
            set
            {
                if (SetProperty(ref _tenNganHangChuHome, value))
                {
                    OnPropertyChanged(nameof(NganHangHienThi));
                }
            }
        }

        private string _soTaiKhoanChuHome = string.Empty;
        public string SoTaiKhoanChuHome
        {
            get => _soTaiKhoanChuHome;
            set
            {
                if (SetProperty(ref _soTaiKhoanChuHome, value))
                {
                    OnPropertyChanged(nameof(SoTaiKhoanHienThi));
                }
            }
        }

        private string _chuTaiKhoanChuHome = string.Empty;
        public string ChuTaiKhoanChuHome
        {
            get => _chuTaiKhoanChuHome;
            set
            {
                if (SetProperty(ref _chuTaiKhoanChuHome, value))
                {
                    OnPropertyChanged(nameof(ChuTaiKhoanHienThi));
                }
            }
        }

        public string ChuTaiKhoanHienThi => !string.IsNullOrWhiteSpace(ChuTaiKhoanChuHome) ? ChuTaiKhoanChuHome : (!string.IsNullOrWhiteSpace(TenChuHome) ? TenChuHome : "Chưa cập nhật");
        public string CCCDHienThi => !string.IsNullOrWhiteSpace(SoCCCDChuHome) ? SoCCCDChuHome : "Chưa cập nhật";
        public string NganHangHienThi => !string.IsNullOrWhiteSpace(TenNganHangChuHome) ? TenNganHangChuHome : "Chưa cập nhật";
        public string SoTaiKhoanHienThi => !string.IsNullOrWhiteSpace(SoTaiKhoanChuHome) ? SoTaiKhoanChuHome : "Chưa cập nhật";

        // ================= TRẠNG THÁI & PHÊ DUYỆT =================
        private string _trangThai = "ChoDuyet";
        public string TrangThai
        {
            get => _trangThai;
            set
            {
                if (SetProperty(ref _trangThai, value))
                {
                    OnPropertyChanged(nameof(TrangThaiDuyet));
                    OnPropertyChanged(nameof(TenHienThiTrangThai));
                    OnPropertyChanged(nameof(MauTrangThai));
                    OnPropertyChanged(nameof(CoTheDuyet));
                }
            }
        }

        public string TrangThaiDuyet
        {
            get => TrangThai;
            set => TrangThai = value;
        }

        private string? _lyDoTuChoi;
        public string? LyDoTuChoi
        {
            get => _lyDoTuChoi;
            set => SetProperty(ref _lyDoTuChoi, value);
        }

        public DateTime NgayGuiDuyet { get; set; } = DateTime.Now;
        public DateTime? NgayDuyet { get; set; }
        public string? NguoiDuyet { get; set; }

        // ================= HỒ SƠ PHÁP LÝ & HÌNH ẢNH =================
        private string _hinhAnhDaiDien = string.Empty;
        public string HinhAnhDaiDien
        {
            get => _hinhAnhDaiDien;
            set => SetProperty(ref _hinhAnhDaiDien, value);
        }

        private string _hinhAnhGiayPhepKinhDoanh = string.Empty;
        public string HinhAnhGiayPhepKinhDoanh
        {
            get => _hinhAnhGiayPhepKinhDoanh;
            set => SetProperty(ref _hinhAnhGiayPhepKinhDoanh, value);
        }

        private string _hinhAnhPCCC = string.Empty;
        public string HinhAnhPCCC
        {
            get => _hinhAnhPCCC;
            set => SetProperty(ref _hinhAnhPCCC, value);
        }

        private string _hinhAnhANTT = string.Empty;
        public string HinhAnhANTT
        {
            get => _hinhAnhANTT;
            set
            {
                if (SetProperty(ref _hinhAnhANTT, value))
                {
                    OnPropertyChanged(nameof(HinhAnhAnNinhTratTu));
                }
            }
        }

        public string HinhAnhAnNinhTratTu
        {
            get => HinhAnhANTT;
            set => HinhAnhANTT = value;
        }

        public List<string> DanhSachHinhAnh { get; set; } = new();

        private int _tongSoPhong;
        public int TongSoPhong
        {
            get => _tongSoPhong;
            set
            {
                if (SetProperty(ref _tongSoPhong, value))
                {
                    OnPropertyChanged(nameof(SucChuaHienThi));
                }
            }
        }

        private decimal _giaThapNhat;
        public decimal GiaThapNhat
        {
            get => _giaThapNhat;
            set
            {
                if (SetProperty(ref _giaThapNhat, value))
                {
                    OnPropertyChanged(nameof(GiaTrungBinh));
                    OnPropertyChanged(nameof(GiaCoSo));
                    OnPropertyChanged(nameof(KhoangGiaHienThi));
                }
            }
        }

        private decimal _giaCaoNhat;
        public decimal GiaCaoNhat
        {
            get => _giaCaoNhat;
            set
            {
                if (SetProperty(ref _giaCaoNhat, value))
                {
                    OnPropertyChanged(nameof(GiaTrungBinh));
                    OnPropertyChanged(nameof(GiaCoSo));
                    OnPropertyChanged(nameof(KhoangGiaHienThi));
                }
            }
        }

        // ================= THUỘC TÍNH BỔ TRỢ GIAO DIỆN =================
        public string MaCoSoHienThi => $"#CS-{MaCoSo:D5}";
        public string MaHoSoHienThi => $"#HS-AYR-{MaCoSo:D4}";

        private string _loaiHinh = "Homestay";
        public string LoaiHinh
        {
            get => _loaiHinh;
            set
            {
                if (SetProperty(ref _loaiHinh, value))
                {
                    OnPropertyChanged(nameof(LoaiHinhHienThi));
                }
            }
        }

        public string LoaiHinhHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(LoaiHinh)) return "Homestay";
                if (LoaiHinh.Contains("Hotel", StringComparison.OrdinalIgnoreCase) ||
                    LoaiHinh.Contains("KhachSan", StringComparison.OrdinalIgnoreCase) ||
                    LoaiHinh.Contains("Khách sạn", StringComparison.OrdinalIgnoreCase))
                {
                    return "Khách sạn";
                }
                if (LoaiHinh.Contains("Resort", StringComparison.OrdinalIgnoreCase))
                {
                    return "Resort";
                }
                if (LoaiHinh.Contains("Villa", StringComparison.OrdinalIgnoreCase) ||
                    LoaiHinh.Contains("Biệt thự", StringComparison.OrdinalIgnoreCase))
                {
                    return "Villa";
                }
                if (LoaiHinh.Contains("Apartment", StringComparison.OrdinalIgnoreCase) ||
                    LoaiHinh.Contains("Căn hộ", StringComparison.OrdinalIgnoreCase))
                {
                    return "Căn hộ";
                }
                return "Homestay";
            }
            set => LoaiHinh = value;
        }

        public double DanhGia { get; set; } = 0.0;
        public int SoLuotDat { get; set; } = 0;
        public string DanhGiaHienThi => DanhGia > 0 ? $"{DanhGia:F1}" : "Chưa có";
        public string KhuVuc
        {
            get => string.IsNullOrEmpty(TinhThanh) ? "Việt Nam" : TinhThanh;
            set => TinhThanh = value;
        }

        public decimal GiaTrungBinh
        {
            get => GiaThapNhat > 0 && GiaCaoNhat > 0 ? (GiaThapNhat + GiaCaoNhat) / 2m : (GiaThapNhat > 0 ? GiaThapNhat : GiaCaoNhat);
            set
            {
                GiaThapNhat = value;
                GiaCaoNhat = value;
            }
        }

        public decimal GiaCoSo
        {
            get => GiaTrungBinh > 0 ? GiaTrungBinh : (GiaThapNhat > 0 ? GiaThapNhat : (GiaCaoNhat > 0 ? GiaCaoNhat : 0m));
            set => GiaTrungBinh = value;
        }

        public string ThoiGianCho => "---";
        public string ToaDoGPS { get; set; } = string.Empty;
        public string ChuyenVienThamDinh { get; set; } = string.Empty;
        public int SoLuongKhieuNai { get; set; } = 0;
        public string HinhAnhThumbnail1 { get; set; } = string.Empty;
        public string HinhAnhThumbnail2 { get; set; } = string.Empty;

        public string TenHienThiTrangThai
        {
            get => TrangThai switch
            {
                "ChoDuyet" => "Chờ duyệt",
                "DaDuyet" or "HoatDong" => "Đã duyệt",
                "TuChoi" or "BiKhoa" or "TamDung" => "Từ chối / Tạm khóa",
                _ => TrangThai
            };
            set { }
        }

        public string MauTrangThai
        {
            get => TrangThai switch
            {
                "ChoDuyet" => "#F59E0B", // Vàng cam
                "DaDuyet" or "HoatDong" => "#10B981",  // Xanh lá
                "TuChoi" or "BiKhoa" or "TamDung" => "#EF4444",   // Đỏ
                _ => "#6B7280"
            };
            set { }
        }

        public string KhoangGiaHienThi
        {
            get
            {
                if (GiaThapNhat > 0 && GiaCaoNhat > 0 && GiaThapNhat != GiaCaoNhat)
                    return $"{GiaThapNhat:N0} đ - {GiaCaoNhat:N0} đ / đêm";
                if (GiaCoSo > 0)
                    return $"{GiaCoSo:N0} đ / đêm";
                return "Đang cập nhật";
            }
            set { }
        }

        public string SucChuaHienThi
        {
            get => TongSoPhong > 0 ? $"{TongSoPhong * 2} - {TongSoPhong * 4} Khách" : "Đang cập nhật";
            set { }
        }

        public bool CoTheDuyet
        {
            get => TrangThai == "ChoDuyet";
            set { }
        }
    }
}
