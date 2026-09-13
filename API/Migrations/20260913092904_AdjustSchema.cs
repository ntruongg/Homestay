using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AdjustSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_DonDatPhong_DonDatPhongMaDonDatPhong",
                table: "DanhGia");

            migrationBuilder.DropIndex(
                name: "IX_DanhGia_DonDatPhongMaDonDatPhong",
                table: "DanhGia");

            migrationBuilder.DropColumn(
                name: "DonDatPhongMaDonDatPhong",
                table: "DanhGia");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_DonDatPhong_MaDonDatPhong",
                table: "DanhGia",
                column: "MaDonDatPhong",
                principalTable: "DonDatPhong",
                principalColumn: "MaDonDatPhong",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DanhGia_DonDatPhong_MaDonDatPhong",
                table: "DanhGia");

            migrationBuilder.AddColumn<int>(
                name: "DonDatPhongMaDonDatPhong",
                table: "DanhGia",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGia_DonDatPhongMaDonDatPhong",
                table: "DanhGia",
                column: "DonDatPhongMaDonDatPhong");

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGia_DonDatPhong_DonDatPhongMaDonDatPhong",
                table: "DanhGia",
                column: "DonDatPhongMaDonDatPhong",
                principalTable: "DonDatPhong",
                principalColumn: "MaDonDatPhong",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
