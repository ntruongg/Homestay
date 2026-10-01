using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDichVuModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhuThu");

            migrationBuilder.CreateTable(
                name: "DichVu",
                columns: table => new
                {
                    MaDichVu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaCoSoLuuTru = table.Column<int>(type: "int", nullable: false),
                    TenDichVu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GiaDichVu = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DichVu", x => x.MaDichVu);
                    table.ForeignKey(
                        name: "FK_DichVu_CoSoLuuTru_MaCoSoLuuTru",
                        column: x => x.MaCoSoLuuTru,
                        principalTable: "CoSoLuuTru",
                        principalColumn: "MaCoSoLuuTru",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DonDatPhongDichVu",
                columns: table => new
                {
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false),
                    MaDichVu = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonDatPhongDichVu", x => new { x.MaDonDatPhong, x.MaDichVu });
                    table.ForeignKey(
                        name: "FK_DonDatPhongDichVu_DichVu_MaDichVu",
                        column: x => x.MaDichVu,
                        principalTable: "DichVu",
                        principalColumn: "MaDichVu",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DonDatPhongDichVu_DonDatPhong_MaDonDatPhong",
                        column: x => x.MaDonDatPhong,
                        principalTable: "DonDatPhong",
                        principalColumn: "MaDonDatPhong",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DichVu_MaCoSoLuuTru",
                table: "DichVu",
                column: "MaCoSoLuuTru");

            migrationBuilder.CreateIndex(
                name: "IX_DonDatPhongDichVu_MaDichVu",
                table: "DonDatPhongDichVu",
                column: "MaDichVu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DonDatPhongDichVu");

            migrationBuilder.DropTable(
                name: "DichVu");

            migrationBuilder.CreateTable(
                name: "PhuThu",
                columns: table => new
                {
                    MaPhuThu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonDatPhong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    TenPhuThu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(12,2)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_PhuThu_MaDonDatPhong",
                table: "PhuThu",
                column: "MaDonDatPhong");
        }
    }
}
