using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoaiPhong",
                columns: table => new
                {
                    MaLoaiPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LoaiPhon__2302121759892A37", x => x.MaLoaiPhong);
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDangNhap = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    GioiTinh = table.Column<string>(type: "char(1)", unicode: false, fixedLength: true, maxLength: 1, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MatKhau = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VaiTro = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TrangThai = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    NgayTao = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TaiKhoan__AD7C6529A22F8D9F", x => x.MaTaiKhoan);
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
                    table.PrimaryKey("PK__TienNghi__ED7B8F4D6A96D1D7", x => x.MaTienNghi);
                });

            migrationBuilder.CreateTable(
                name: "ChuCoSoLuuTru",
                columns: table => new
                {
                    MaChuCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    ThongTinNganHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CCCD = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChuCoSoL__BA45510BE67FAC15", x => x.MaChuCoSoLuuTru);
                    table.ForeignKey(
                        name: "FK__ChuCoSoLu__MaChu__440B1D61",
                        column: x => x.MaChuCoSoLuuTru,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKhachHang = table.Column<int>(type: "int", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KhachHan__88D2F0E54E69A6E4", x => x.MaKhachHang);
                    table.ForeignKey(
                        name: "FK__KhachHang__MaKha__403A8C7D",
                        column: x => x.MaKhachHang,
                        principalTable: "TaiKhoan",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "CoSoLuuTru",
                columns: table => new
                {
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaChuCoSoLuuTru = table.Column<int>(type: "int", nullable: true),
                    TenCoSoLuuTru = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrangThaiDuyet = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LyDoTuChoi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GiayPhepKD_URL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GiayToPCCC_URL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GiayToANTT_URL = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LoaiHinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Homestay")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CoSoLuuT__ED7C1678F326FE9E", x => x.MaCoSoLuuTru);
                    table.ForeignKey(
                        name: "FK__CoSoLuuTr__MaChu__46E78A0C",
                        column: x => x.MaChuCoSoLuuTru,
                        principalTable: "ChuCoSoLuuTru",
                        principalColumn: "MaChuCoSoLuuTru");
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
                    table.PrimaryKey("PK__CoSoLuuT__23ABAE8CC4C28605", x => new { x.MaCoSoLuuTru, x.MaTienNghi });
                    table.ForeignKey(
                        name: "FK__CoSoLuuTr__MaCoS__6D0D32F4",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru");
                    table.ForeignKey(
                        name: "FK__CoSoLuuTr__MaTie__6E01572D",
                        column: x => x.MaTienNghi,
                        principalTable: "TienNghi",
                        principalColumn: "MaTienNghi");
                });

            migrationBuilder.CreateTable(
                name: "Phong",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: true),
                    SoPhong = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SucChua = table.Column<int>(type: "int", nullable: true),
                    MaLoaiPhong = table.Column<int>(type: "int", nullable: true),
                    TinhTrang = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GiaHienTai = table.Column<decimal>(type: "decimal(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Phong__20BD5E5BE22A6455", x => x.MaPhong);
                    table.ForeignKey(
                        name: "FK__Phong__MaCoSoLuu__4D94879B",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru");
                    table.ForeignKey(
                        name: "FK__Phong__MaLoaiPho__4F7CD00D",
                        column: x => x.MaLoaiPhong,
                        principalTable: "LoaiPhong",
                        principalColumn: "MaLoaiPhong");
                });

            migrationBuilder.CreateTable(
                name: "DonDatPhong",
                columns: table => new
                {
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKhachHang = table.Column<int>(type: "int", nullable: true),
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: true),
                    MaPhong = table.Column<int>(type: "int", nullable: true),
                    NgayDat = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(getdate())"),
                    NgayDen = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayDi = table.Column<DateOnly>(type: "date", nullable: true),
                    SoNguoi = table.Column<int>(type: "int", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(12,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DonDatPh__F7BDF64703B8AFAB", x => x.MaDonDatPhong);
                    table.ForeignKey(
                        name: "FK__DonDatPho__MaCoS__5441852A",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru");
                    table.ForeignKey(
                        name: "FK__DonDatPho__MaKha__534D60F1",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang");
                    table.ForeignKey(
                        name: "FK__DonDatPho__MaPho__5535A963",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateTable(
                name: "HinhAnh",
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
                    table.PrimaryKey("PK__HinhAnh__A9C37A9BF11BCA99", x => x.MaHinhAnh);
                    table.ForeignKey(
                        name: "FK__HinhAnh__MaCoSoL__74AE54BC",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru");
                    table.ForeignKey(
                        name: "FK__HinhAnh__MaPhong__75A278F5",
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
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: true),
                    MaPhong = table.Column<int>(type: "int", nullable: true),
                    Ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true, defaultValue: "Trống")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__LichLuuT__728A9AE9008A3495", x => x.MaLich);
                    table.ForeignKey(
                        name: "FK__LichLuuTr__MaCoS__6477ECF3",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru");
                    table.ForeignKey(
                        name: "FK__LichLuuTr__MaPho__656C112C",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateTable(
                name: "Phong_TienNghi",
                columns: table => new
                {
                    MaPhong = table.Column<int>(type: "int", nullable: false),
                    MaTienNghi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Phong_Ti__EE6AE6AF68B697AC", x => new { x.MaPhong, x.MaTienNghi });
                    table.ForeignKey(
                        name: "FK__Phong_Tie__MaPho__70DDC3D8",
                        column: x => x.MaPhong,
                        principalTable: "Phong",
                        principalColumn: "MaPhong");
                    table.ForeignKey(
                        name: "FK__Phong_Tie__MaTie__71D1E811",
                        column: x => x.MaTienNghi,
                        principalTable: "TienNghi",
                        principalColumn: "MaTienNghi");
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
                    PhanHoiChuCoSoLuuTru = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__DanhGia__AA9515BF4288ACD0", x => x.MaDanhGia);
                    table.ForeignKey(
                        name: "FK__DanhGia__MaDonDa__797309D9",
                        column: x => x.MaDonDatPhong,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong");
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    MaHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: true),
                    NgayLap = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(getdate())"),
                    TongTien = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__HoaDon__835ED13B867BCACF", x => x.MaHoaDon);
                    table.ForeignKey(
                        name: "FK__HoaDon__MaDonDat__5BE2A6F2",
                        column: x => x.MaDonDatPhong,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong");
                });

            migrationBuilder.CreateTable(
                name: "ChiTietHoaDon",
                columns: table => new
                {
                    MaChiTiet = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoaDon = table.Column<int>(type: "int", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: true),
                    ThanhTien = table.Column<decimal>(type: "decimal(23,2)", nullable: true, computedColumnSql: "([DonGia]*[SoLuong])", stored: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ChiTietH__CDF0A114A3164D4E", x => x.MaChiTiet);
                    table.ForeignKey(
                        name: "FK__ChiTietHo__MaHoa__5FB337D6",
                        column: x => x.MaHoaDon,
                        principalTable: "HoaDon",
                        principalColumn: "MaHoaDon");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietHoaDon_MaHoaDon",
                table: "ChiTietHoaDon",
                column: "MaHoaDon");

            migrationBuilder.CreateIndex(
                name: "UQ__ChuCoSoL__A955A0AAB7449204",
                table: "ChuCoSoLuuTru",
                column: "CCCD",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTru_MaChuCoSoLuuTru",
                table: "CoSoLuuTru",
                column: "MaChuCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_CoSoLuuTru_TienNghi_MaTienNghi",
                table: "CoSoLuuTru_TienNghi",
                column: "MaTienNghi");

            migrationBuilder.CreateIndex(
                name: "UQ__DanhGia__F7BDF64622A9B2B1",
                table: "DanhGia",
                column: "MaDonDatPhong",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonDatPhong_MaCoSoLuuTru",
                table: "DonDatPhong",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatPhong_MaKhachHang",
                table: "DonDatPhong",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatPhong_MaPhong",
                table: "DonDatPhong",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnh_MaCoSoLuuTru",
                table: "HinhAnh",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnh_MaPhong",
                table: "HinhAnh",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaDonDatPhong",
                table: "HoaDon",
                column: "MaDonDatPhong");

            migrationBuilder.CreateIndex(
                name: "UQ_LichHomestay_Ngay",
                table: "LichLuuTru",
                columns: new[] { "MaCoSoLuuTru", "Ngay" },
                unique: true,
                filter: "[MaCoSoLuuTru] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_LichPhong_Ngay",
                table: "LichLuuTru",
                columns: new[] { "MaPhong", "Ngay" },
                unique: true,
                filter: "[MaPhong] IS NOT NULL");

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
                name: "UQ__TaiKhoan__55F68FC0651FA140",
                table: "TaiKhoan",
                column: "TenDangNhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__TaiKhoan__5C7E359E06BB0A97",
                table: "TaiKhoan",
                column: "Phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__TaiKhoan__A9D105348E21A727",
                table: "TaiKhoan",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietHoaDon");

            migrationBuilder.DropTable(
                name: "CoSoLuuTru_TienNghi");

            migrationBuilder.DropTable(
                name: "DanhGia");

            migrationBuilder.DropTable(
                name: "HinhAnh");

            migrationBuilder.DropTable(
                name: "LichLuuTru");

            migrationBuilder.DropTable(
                name: "Phong_TienNghi");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "TienNghi");

            migrationBuilder.DropTable(
                name: "DonDatPhong");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "Phong");

            migrationBuilder.DropTable(
                name: "CoSoLuuTru");

            migrationBuilder.DropTable(
                name: "LoaiPhong");

            migrationBuilder.DropTable(
                name: "ChuCoSoLuuTru");

            migrationBuilder.DropTable(
                name: "TaiKhoan");
        }
    }
}
