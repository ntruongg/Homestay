using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                name: "TienNghi",
                columns: table => new
                {
                    MaTienNghi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTienNghi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienNghi", x => x.MaTienNghi);
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
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "date", nullable: true),
                    GioiTinh = table.Column<string>(type: "char(1)", maxLength: 1, nullable: true),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaVaiTro = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "date", nullable: false),
                    ThongTinNganHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTaiKhoan);
                    table.ForeignKey(
                        name: "FK_TaiKhoan_VaiTro_MaVaiTro",
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
                    GiayPhepKD_URL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GiayToPCCC_URL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GiayToANTT_URL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LoaiHinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChinhSach = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoSoLuuTru", x => x.MaCoSoLuuTru);
                    table.ForeignKey(
                        name: "FK_CoSoLuuTru_TaiKhoan_MaChuCoSoLuuTru",
                        column: x => x.MaChuCoSoLuuTru,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
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
                    NgayDat = table.Column<DateTime>(type: "date", nullable: false),
                    NgayDen = table.Column<DateTime>(type: "date", nullable: false),
                    NgayDi = table.Column<DateTime>(type: "date", nullable: false),
                    SoNguoiLon = table.Column<int>(type: "int", nullable: false),
                    SoTreEm = table.Column<int>(type: "int", nullable: false),
                    SoNguoi = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
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
                        name: "FK_DonDatPhong_TaiKhoan_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoSoLuuTru_TienNghi",
                columns: table => new
                {
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    MaTienNghi = table.Column<int>(type: "int", nullable: false),
                    CoSoLuuTruMaCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    TienNghiMaTienNghi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoSoLuuTru_TienNghi", x => new { x.MaCoSoLuuTru, x.MaTienNghi });
                    table.ForeignKey(
                        name: "FK_CoSoLuuTru_TienNghi_CoSoLuuTru_CoSoLuuTruMaCoSoLuuTru",
                        column: x => x.CoSoLuuTruMaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoSoLuuTru_TienNghi_TienNghi_TienNghiMaTienNghi",
                        column: x => x.TienNghiMaTienNghi,
                        principalTable: "TienNghi",
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
                        name: "FK_LichSuDuyet_TaiKhoan_MaNguoiDuyet",
                        column: x => x.MaNguoiDuyet,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan",
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
                    SucChua = table.Column<int>(type: "int", nullable: false),
                    MaLoaiPhong = table.Column<int>(type: "int", nullable: true),
                    TinhTrang = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GiaGoc = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
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
                        principalColumn: "MaLoaiPhong");
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
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGia", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK_DanhGia_DonDatPhong_MaDonDatPhong",
                        column: x => x.MaDonDatPhong,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong",
                        onDelete: ReferentialAction.Restrict);
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
                    PTTT = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToan", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_ThanhToan_DonDatPhong_MaHoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDon",
                columns: table => new
                {
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false),
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    DonDatPhongMaDonDatPhong = table.Column<int>(type: "int", nullable: false),
                    PhongMaPhong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDon", x => new { x.MaDonDatPhong, x.MaPhong });
                    table.ForeignKey(
                        name: "FK_ChiTietDon_DonDatPhong_DonDatPhongMaDonDatPhong",
                        column: x => x.DonDatPhongMaDonDatPhong,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDon_Phong_PhongMaPhong",
                        column: x => x.PhongMaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnh",
                columns: table => new
                {
                    MaHinhAnh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: true),
                    CoSoLuuTruMaCoSoLuuTru = table.Column<int>(type: "int", nullable: true),
                    MaPhong = table.Column<int>(type: "int", nullable: true),
                    PhongMaPhong = table.Column<int>(type: "int", nullable: true),
                    UrlHinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnh", x => x.MaHinhAnh);
                    table.ForeignKey(
                        name: "FK_HinhAnh_CoSoLuuTru_CoSoLuuTruMaCoSoLuuTru",
                        column: x => x.CoSoLuuTruMaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru");
                    table.ForeignKey(
                        name: "FK_HinhAnh_Phong_PhongMaPhong",
                        column: x => x.PhongMaPhong,
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
                    PhongMaPhong = table.Column<int>(type: "int", nullable: false),
                    Ngay = table.Column<DateTime>(type: "date", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichLuuTru", x => x.MaLich);
                    table.ForeignKey(
                        name: "FK_LichLuuTru_Phong_PhongMaPhong",
                        column: x => x.PhongMaPhong,
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
                    PhongMaPhong = table.Column<int>(type: "int", nullable: false),
                    TienNghiMaTienNghi = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phong_TienNghi", x => new { x.MaPhong, x.MaTienNghi });
                    table.ForeignKey(
                        name: "FK_Phong_TienNghi_Phong_PhongMaPhong",
                        column: x => x.PhongMaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Phong_TienNghi_TienNghi_TienNghiMaTienNghi",
                        column: x => x.TienNghiMaTienNghi,
                        principalTable: "TienNghi",
                        principalColumn: "MaTienNghi",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "VaiTro",
                columns: new[] { "MaVaiTro", "MoTa", "TenVaiTro" },
                values: new object[,]
                {
                    { 1, "Traveler / Guest", "GUEST" },
                    { 2, "Homestay Host / Owner", "OWNER" },
                    { 3, "System Administrator", "ADMIN" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDon_DonDatPhongMaDonDatPhong",
                table: "ChiTietDon",
                column: "DonDatPhongMaDonDatPhong");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDon_PhongMaPhong",
                table: "ChiTietDon",
                column: "PhongMaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTru_MaChuCoSoLuuTru",
                table: "CoSoLuuTru",
                column: "MaChuCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTru_TienNghi_CoSoLuuTruMaCoSoLuuTru",
                table: "CoSoLuuTru_TienNghi",
                column: "CoSoLuuTruMaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTru_TienNghi_TienNghiMaTienNghi",
                table: "CoSoLuuTru_TienNghi",
                column: "TienNghiMaTienNghi");

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
                name: "IX_HinhAnh_CoSoLuuTruMaCoSoLuuTru",
                table: "HinhAnh",
                column: "CoSoLuuTruMaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnh_PhongMaPhong",
                table: "HinhAnh",
                column: "PhongMaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_LichLuuTru_MaPhong_Ngay",
                table: "LichLuuTru",
                columns: new[] { "MaPhong", "Ngay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichLuuTru_PhongMaPhong",
                table: "LichLuuTru",
                column: "PhongMaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDuyet_MaCoSoLuuTru",
                table: "LichSuDuyet",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuDuyet_MaNguoiDuyet",
                table: "LichSuDuyet",
                column: "MaNguoiDuyet");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_MaCoSoLuuTru",
                table: "Phong",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_MaLoaiPhong",
                table: "Phong",
                column: "MaLoaiPhong");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_TienNghi_PhongMaPhong",
                table: "Phong_TienNghi",
                column: "PhongMaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_TienNghi_TienNghiMaTienNghi",
                table: "Phong_TienNghi",
                column: "TienNghiMaTienNghi");

            migrationBuilder.CreateIndex(
                name: "IX_PhuThu_MaDonDatPhong",
                table: "PhuThu",
                column: "MaDonDatPhong");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_DienThoai",
                table: "TaiKhoan",
                column: "DienThoai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_Email",
                table: "TaiKhoan",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_MaVaiTro",
                table: "TaiKhoan",
                column: "MaVaiTro");
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
                name: "Phong_TienNghi");

            migrationBuilder.DropTable(
                name: "PhuThu");

            migrationBuilder.DropTable(
                name: "ThanhToan");

            migrationBuilder.DropTable(
                name: "Phong");

            migrationBuilder.DropTable(
                name: "TienNghi");

            migrationBuilder.DropTable(
                name: "DonDatPhong");

            migrationBuilder.DropTable(
                name: "CoSoLuuTru");

            migrationBuilder.DropTable(
                name: "LoaiPhong");

            migrationBuilder.DropTable(
                name: "GiamGia");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "VaiTro");
        }
    }
}
