using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialNormalizedV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiamGia",
                columns: table => new
                {
                    MaGiamGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhanTram = table.Column<int>(type: "int", nullable: false),
                    ToiDa = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "date", nullable: true),
                    NgayHetHan = table.Column<DateTime>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiamGia", x => x.MaGiamGia);
                });

            migrationBuilder.CreateTable(
                name: "LoaiPhong",
                columns: table => new
                {
                    MaLoaiPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiPhong", x => x.MaLoaiPhong);
                });

            migrationBuilder.CreateTable(
                name: "TienNghiCoSo",
                columns: table => new
                {
                    MaTienNghi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTienNghi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienNghiCoSo", x => x.MaTienNghi);
                });

            migrationBuilder.CreateTable(
                name: "TienNghiPhong",
                columns: table => new
                {
                    MaTienNghi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTienNghi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienNghiPhong", x => x.MaTienNghi);
                });

            migrationBuilder.CreateTable(
                name: "VaiTro",
                columns: table => new
                {
                    MaVaiTro = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenVaiTro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VaiTro", x => x.MaVaiTro);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDung",
                columns: table => new
                {
                    MaNguoiDung = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: true),
                    GioiTinh = table.Column<string>(type: "char(1)", maxLength: 1, nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MaVaiTro = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime", nullable: false),
                    NganHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoTaiKhoan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TenNguoiThuHuong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDung", x => x.MaNguoiDung);
                    table.ForeignKey(
                        name: "FK_NguoiDung_VaiTro_MaVaiTro",
                        column: x => x.MaVaiTro,
                        principalTable: "VaiTro",
                        principalColumn: "MaVaiTro",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoSoLuuTru",
                columns: table => new
                {
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChuCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    TenCoSoLuuTru = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhuongXa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ThanhPho = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GiayPhepKD_URL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GiayToPCCC_URL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GiayToANTT_URL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LoaiHinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChinhSach = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TrangThaiDuyet = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoSoLuuTru", x => x.MaCoSoLuuTru);
                    table.ForeignKey(
                        name: "FK_CoSoLuuTru_NguoiDung_MaChuCoSoLuuTru",
                        column: x => x.MaChuCoSoLuuTru,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonDatPhong",
                columns: table => new
                {
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    MaGiamGia = table.Column<int>(type: "int", nullable: true),
                    NgayDat = table.Column<DateTime>(type: "datetime", nullable: false),
                    NgayDen = table.Column<DateTime>(type: "date", nullable: false),
                    NgayDi = table.Column<DateTime>(type: "date", nullable: false),
                    SoNguoiLon = table.Column<int>(type: "int", nullable: false),
                    SoTreEm = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ThoiGianYeuCauHoan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoHoanTien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonDatPhong", x => x.MaDonDatPhong);
                    table.ForeignKey(
                        name: "FK_DonDatPhong_GiamGia_MaGiamGia",
                        column: x => x.MaGiamGia,
                        principalTable: "GiamGia",
                        principalColumn: "MaGiamGia",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonDatPhong_NguoiDung_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NhatKyHoatDong",
                columns: table => new
                {
                    MaNhatKy = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaNguoiDung = table.Column<int>(type: "int", nullable: true),
                    HanhDong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LoaiDoiTuong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaDoiTuong = table.Column<int>(type: "int", nullable: true),
                    MoTaChiTiet = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DiaChiIP = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThoiGian = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyHoatDong", x => x.MaNhatKy);
                    table.ForeignKey(
                        name: "FK_NhatKyHoatDong_NguoiDung_MaNguoiDung",
                        column: x => x.MaNguoiDung,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CoSoLuuTru_TienNghi",
                columns: table => new
                {
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    MaTienNghi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoSoLuuTru_TienNghi", x => new { x.MaCoSoLuuTru, x.MaTienNghi });
                    table.ForeignKey(
                        name: "FK_CoSoLuuTru_TienNghi_CoSoLuuTru_MaCoSoLuuTru",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoSoLuuTru_TienNghi_TienNghiCoSo_MaTienNghi",
                        column: x => x.MaTienNghi,
                        principalTable: "TienNghiCoSo",
                        principalColumn: "MaTienNghi",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuDuyet",
                columns: table => new
                {
                    MaLichSu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    MaNguoiDuyet = table.Column<int>(type: "int", nullable: true),
                    TrangThaiDuyet = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LyDoTuChoi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NgayDuyet = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuDuyet", x => x.MaLichSu);
                    table.ForeignKey(
                        name: "FK_LichSuDuyet_CoSoLuuTru_MaCoSoLuuTru",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LichSuDuyet_NguoiDung_MaNguoiDuyet",
                        column: x => x.MaNguoiDuyet,
                        principalTable: "NguoiDung",
                        principalColumn: "MaNguoiDung",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Phong",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    SoPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaLoaiPhong = table.Column<int>(type: "int", nullable: true),
                    SucChuaNguoiLon = table.Column<int>(type: "int", nullable: false),
                    SucChuaTreEm = table.Column<int>(type: "int", nullable: false),
                    GiaGoc = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    MoTaPhong = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TinhTrang = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phong", x => x.MaPhong);
                    table.ForeignKey(
                        name: "FK_Phong_CoSoLuuTru_MaCoSoLuuTru",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Phong_LoaiPhong_MaLoaiPhong",
                        column: x => x.MaLoaiPhong,
                        principalTable: "LoaiPhong",
                        principalColumn: "MaLoaiPhong",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DanhGia",
                columns: table => new
                {
                    MaDanhGia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false),
                    DiemSo = table.Column<int>(type: "int", nullable: false),
                    NoiDungDanhGia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime", nullable: false),
                    PhanHoiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayPhanHoi = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGia_DonDatPhong_MaDonDatPhong",
                        column: x => x.MaDonDatPhong,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhuThu",
                columns: table => new
                {
                    MaPhuThu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false),
                    TenPhuThu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuThu", x => x.MaPhuThu);
                    table.ForeignKey(
                        name: "FK_PhuThu_DonDatPhong_MaDonDatPhong",
                        column: x => x.MaDonDatPhong,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToan",
                columns: table => new
                {
                    MaHoaDon = table.Column<int>(type: "int", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TienGoc = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    PTTT = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime", nullable: false),
                    PhanTramHoaHong = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TienHoaHong = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    TienThucNhanChu = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToan", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_ThanhToan_DonDatPhong_MaHoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDon",
                columns: table => new
                {
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false),
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDon", x => new { x.MaDonDatPhong, x.MaPhong });
                    table.ForeignKey(
                        name: "FK_ChiTietDon_DonDatPhong_MaDonDatPhong",
                        column: x => x.MaDonDatPhong,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDon_Phong_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnh",
                columns: table => new
                {
                    MaHinhAnh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: true),
                    MaPhong = table.Column<int>(type: "int", nullable: true),
                    UrlHinhAnh = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnh", x => x.MaHinhAnh);
                    table.ForeignKey(
                        name: "FK_HinhAnh_CoSoLuuTru_MaCoSoLuuTru",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HinhAnh_Phong_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateTable(
                name: "LichLuuTru",
                columns: table => new
                {
                    MaLich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    Ngay = table.Column<DateTime>(type: "date", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichLuuTru", x => x.MaLich);
                    table.ForeignKey(
                        name: "FK_LichLuuTru_Phong_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Phong_TienNghi",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    MaTienNghi = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phong_TienNghi", x => new { x.MaPhong, x.MaTienNghi });
                    table.ForeignKey(
                        name: "FK_Phong_TienNghi_Phong_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Phong_TienNghi_TienNghiPhong_MaTienNghi",
                        column: x => x.MaTienNghi,
                        principalTable: "TienNghiPhong",
                        principalColumn: "MaTienNghi",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDon_MaPhong",
                table: "ChiTietDon",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTru_MaChuCoSoLuuTru",
                table: "CoSoLuuTru",
                column: "MaChuCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTru_TienNghi_MaTienNghi",
                table: "CoSoLuuTru_TienNghi",
                column: "MaTienNghi");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_MaDonDatPhong",
                table: "DanhGia",
                column: "MaDonDatPhong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonDatPhong_MaGiamGia",
                table: "DonDatPhong",
                column: "MaGiamGia");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatPhong_MaKhachHang",
                table: "DonDatPhong",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_GiamGia_TenMa",
                table: "GiamGia",
                column: "TenMa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnh_MaCoSoLuuTru",
                table: "HinhAnh",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnh_MaPhong",
                table: "HinhAnh",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_LichLuuTru_MaPhong_Ngay",
                table: "LichLuuTru",
                columns: new[] { "MaPhong", "Ngay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDuyet_MaCoSoLuuTru",
                table: "LichSuDuyet",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDuyet_MaNguoiDuyet",
                table: "LichSuDuyet",
                column: "MaNguoiDuyet");

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_DienThoai",
                table: "NguoiDung",
                column: "DienThoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_Email",
                table: "NguoiDung",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_MaVaiTro",
                table: "NguoiDung",
                column: "MaVaiTro");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_MaNguoiDung",
                table: "NhatKyHoatDong",
                column: "MaNguoiDung");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyHoatDong_ThoiGian",
                table: "NhatKyHoatDong",
                column: "ThoiGian");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_MaCoSoLuuTru",
                table: "Phong",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_MaLoaiPhong",
                table: "Phong",
                column: "MaLoaiPhong");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_TienNghi_MaTienNghi",
                table: "Phong_TienNghi",
                column: "MaTienNghi");

            migrationBuilder.CreateIndex(
                name: "IX_PhuThu_MaDonDatPhong",
                table: "PhuThu",
                column: "MaDonDatPhong");

            migrationBuilder.CreateIndex(
                name: "IX_TienNghiCoSo_TenTienNghi",
                table: "TienNghiCoSo",
                column: "TenTienNghi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TienNghiPhong_TenTienNghi",
                table: "TienNghiPhong",
                column: "TenTienNghi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VaiTro_TenVaiTro",
                table: "VaiTro",
                column: "TenVaiTro",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDon");

            migrationBuilder.DropTable(
                name: "CoSoLuuTru_TienNghi");

            migrationBuilder.DropTable(
                name: "DanhGia");

            migrationBuilder.DropTable(
                name: "HinhAnh");

            migrationBuilder.DropTable(
                name: "LichLuuTru");

            migrationBuilder.DropTable(
                name: "LichSuDuyet");

            migrationBuilder.DropTable(
                name: "NhatKyHoatDong");

            migrationBuilder.DropTable(
                name: "Phong_TienNghi");

            migrationBuilder.DropTable(
                name: "PhuThu");

            migrationBuilder.DropTable(
                name: "ThanhToan");

            migrationBuilder.DropTable(
                name: "TienNghiCoSo");

            migrationBuilder.DropTable(
                name: "Phong");

            migrationBuilder.DropTable(
                name: "TienNghiPhong");

            migrationBuilder.DropTable(
                name: "DonDatPhong");

            migrationBuilder.DropTable(
                name: "CoSoLuuTru");

            migrationBuilder.DropTable(
                name: "LoaiPhong");

            migrationBuilder.DropTable(
                name: "GiamGia");

            migrationBuilder.DropTable(
                name: "NguoiDung");

            migrationBuilder.DropTable(
                name: "VaiTro");
        }
    }
}
