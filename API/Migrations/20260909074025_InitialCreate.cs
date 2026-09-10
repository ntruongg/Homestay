using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoaiPhongs",
                columns: table => new
                {
                    MaLoaiPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiPhongs", x => x.MaLoaiPhong);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoans",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoans", x => x.MaTaiKhoan);
                });

            migrationBuilder.CreateTable(
                name: "TienNghis",
                columns: table => new
                {
                    MaTienNghi = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenTienNghi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TienNghis", x => x.MaTienNghi);
                });

            migrationBuilder.CreateTable(
                name: "ChuCoSoLuuTrus",
                columns: table => new
                {
                    MaChuCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    ThongTinNganHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CCCD = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChuCoSoLuuTrus", x => x.MaChuCoSoLuuTru);
                    table.ForeignKey(
                        name: "FK_ChuCoSoLuuTrus_TaiKhoans_MaChuCoSoLuuTru",
                        column: x => x.MaChuCoSoLuuTru,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KhachHangs",
                columns: table => new
                {
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHangs", x => x.MaKhachHang);
                    table.ForeignKey(
                        name: "FK_KhachHangs_TaiKhoans_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "TaiKhoans",
                        principalColumn: "MaTaiKhoan",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoSoLuuTrus",
                columns: table => new
                {
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChuCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    TenCoSoLuuTru = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TrangThaiDuyet = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LoaiHinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoSoLuuTrus", x => x.MaCoSoLuuTru);
                    table.ForeignKey(
                        name: "FK_CoSoLuuTrus_ChuCoSoLuuTrus_MaChuCoSoLuuTru",
                        column: x => x.MaChuCoSoLuuTru,
                        principalTable: "ChuCoSoLuuTrus",
                        principalColumn: "MaChuCoSoLuuTru",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoSoLuuTru_TienNghis",
                columns: table => new
                {
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    MaTienNghi = table.Column<int>(type: "int", nullable: false),
                    CoSoLuuTruMaCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    TienNghiMaTienNghi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoSoLuuTru_TienNghis", x => new { x.MaCoSoLuuTru, x.MaTienNghi });
                    table.ForeignKey(
                        name: "FK_CoSoLuuTru_TienNghis_CoSoLuuTrus_CoSoLuuTruMaCoSoLuuTru",
                        column: x => x.CoSoLuuTruMaCoSoLuuTru,
                        principalTable: "CoSoLuuTrus",
                        principalColumn: "MaCoSoLuuTru",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoSoLuuTru_TienNghis_TienNghis_TienNghiMaTienNghi",
                        column: x => x.TienNghiMaTienNghi,
                        principalTable: "TienNghis",
                        principalColumn: "MaTienNghi",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Phongs",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    SoPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SucChua = table.Column<int>(type: "int", nullable: false),
                    MaLoaiPhong = table.Column<int>(type: "int", nullable: true),
                    TinhTrang = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GiaHienTai = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phongs", x => x.MaPhong);
                    table.ForeignKey(
                        name: "FK_Phongs_CoSoLuuTrus_MaCoSoLuuTru",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTrus",
                        principalColumn: "MaCoSoLuuTru",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Phongs_LoaiPhongs_MaLoaiPhong",
                        column: x => x.MaLoaiPhong,
                        principalTable: "LoaiPhongs",
                        principalColumn: "MaLoaiPhong");
                });

            migrationBuilder.CreateTable(
                name: "DonDatPhongs",
                columns: table => new
                {
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoNguoi = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonDatPhongs", x => x.MaDonDatPhong);
                    table.ForeignKey(
                        name: "FK_DonDatPhongs_KhachHangs_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHangs",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonDatPhongs_Phongs_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phongs",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhs",
                columns: table => new
                {
                    MaHinhAnh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: true),
                    MaPhong = table.Column<int>(type: "int", nullable: true),
                    UrlHinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhs", x => x.MaHinhAnh);
                    table.ForeignKey(
                        name: "FK_HinhAnhs_CoSoLuuTrus_MaCoSoLuuTru",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTrus",
                        principalColumn: "MaCoSoLuuTru");
                    table.ForeignKey(
                        name: "FK_HinhAnhs_Phongs_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phongs",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateTable(
                name: "LichLuuTrus",
                columns: table => new
                {
                    MaLich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    Ngay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichLuuTrus", x => x.MaLich);
                    table.ForeignKey(
                        name: "FK_LichLuuTrus_Phongs_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "Phongs",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Phong_TienNghis",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    MaTienNghi = table.Column<int>(type: "int", nullable: false),
                    PhongMaPhong = table.Column<int>(type: "int", nullable: false),
                    TienNghiMaTienNghi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phong_TienNghis", x => new { x.MaPhong, x.MaTienNghi });
                    table.ForeignKey(
                        name: "FK_Phong_TienNghis_Phongs_PhongMaPhong",
                        column: x => x.PhongMaPhong,
                        principalTable: "Phongs",
                        principalColumn: "MaPhong",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Phong_TienNghis_TienNghis_TienNghiMaTienNghi",
                        column: x => x.TienNghiMaTienNghi,
                        principalTable: "TienNghis",
                        principalColumn: "MaTienNghi",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDons",
                columns: table => new
                {
                    MaHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false),
                    NgayLap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDons", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDons_DonDatPhongs_MaDonDatPhong",
                        column: x => x.MaDonDatPhong,
                        principalTable: "DonDatPhongs",
                        principalColumn: "MaDonDatPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDons",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoaDon = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietHoaDons", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK_ChiTietHoaDons_HoaDons_MaHoaDon",
                        column: x => x.MaHoaDon,
                        principalTable: "HoaDons",
                        principalColumn: "MaHoaDon",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDons_MaHoaDon",
                table: "ChiTietHoaDons",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "IX_ChuCoSoLuuTrus_CCCD",
                table: "ChuCoSoLuuTrus",
                column: "CCCD",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTru_TienNghis_CoSoLuuTruMaCoSoLuuTru",
                table: "CoSoLuuTru_TienNghis",
                column: "CoSoLuuTruMaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTru_TienNghis_TienNghiMaTienNghi",
                table: "CoSoLuuTru_TienNghis",
                column: "TienNghiMaTienNghi");

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTrus_MaChuCoSoLuuTru",
                table: "CoSoLuuTrus",
                column: "MaChuCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatPhongs_MaKhachHang",
                table: "DonDatPhongs",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatPhongs_MaPhong",
                table: "DonDatPhongs",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhs_MaCoSoLuuTru",
                table: "HinhAnhs",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhs_MaPhong",
                table: "HinhAnhs",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDons_MaDonDatPhong",
                table: "HoaDons",
                column: "MaDonDatPhong");

            migrationBuilder.CreateIndex(
                name: "IX_LichLuuTrus_MaPhong_Ngay",
                table: "LichLuuTrus",
                columns: new[] { "MaPhong", "Ngay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Phong_TienNghis_PhongMaPhong",
                table: "Phong_TienNghis",
                column: "PhongMaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_Phong_TienNghis_TienNghiMaTienNghi",
                table: "Phong_TienNghis",
                column: "TienNghiMaTienNghi");

            migrationBuilder.CreateIndex(
                name: "IX_Phongs_MaCoSoLuuTru",
                table: "Phongs",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_Phongs_MaLoaiPhong",
                table: "Phongs",
                column: "MaLoaiPhong");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_Email",
                table: "TaiKhoans",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_Phone",
                table: "TaiKhoans",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoans_TenDangNhap",
                table: "TaiKhoans",
                column: "TenDangNhap",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietHoaDons");

            migrationBuilder.DropTable(
                name: "CoSoLuuTru_TienNghis");

            migrationBuilder.DropTable(
                name: "HinhAnhs");

            migrationBuilder.DropTable(
                name: "LichLuuTrus");

            migrationBuilder.DropTable(
                name: "Phong_TienNghis");

            migrationBuilder.DropTable(
                name: "HoaDons");

            migrationBuilder.DropTable(
                name: "TienNghis");

            migrationBuilder.DropTable(
                name: "DonDatPhongs");

            migrationBuilder.DropTable(
                name: "KhachHangs");

            migrationBuilder.DropTable(
                name: "Phongs");

            migrationBuilder.DropTable(
                name: "CoSoLuuTrus");

            migrationBuilder.DropTable(
                name: "LoaiPhongs");

            migrationBuilder.DropTable(
                name: "ChuCoSoLuuTrus");

            migrationBuilder.DropTable(
                name: "TaiKhoans");
        }
    }
}
