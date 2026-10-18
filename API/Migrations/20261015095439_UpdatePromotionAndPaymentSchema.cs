using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePromotionAndPaymentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Bảng GiamGia: Bổ sung TenChuongTrinh và SoLuong
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GiamGia') AND name = 'TenChuongTrinh')
                BEGIN
                    ALTER TABLE GiamGia ADD TenChuongTrinh NVARCHAR(200) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('GiamGia') AND name = 'SoLuong')
                BEGIN
                    ALTER TABLE GiamGia ADD SoLuong INT NULL;
                END
            ");

            // 2. Bảng ThanhToan: Bổ sung các trường phục vụ quyết toán tài chính 85/15
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'TrangThaiQuyetToan')
                BEGIN
                    ALTER TABLE ThanhToan ADD TrangThaiQuyetToan NVARCHAR(30) NOT NULL CONSTRAINT DF_ThanhToan_TrangThaiQuyetToan DEFAULT 'ChuaQuyetToan';
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'MaGiaoDichQuyetToan')
                BEGIN
                    ALTER TABLE ThanhToan ADD MaGiaoDichQuyetToan VARCHAR(50) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'NgayQuyetToan')
                BEGIN
                    ALTER TABLE ThanhToan ADD NgayQuyetToan DATETIME2 NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'SoTienQuyetToan')
                BEGIN
                    ALTER TABLE ThanhToan ADD SoTienQuyetToan DECIMAL(12,2) NULL;
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('ThanhToan') AND name = 'GhiChuQuyetToan')
                BEGIN
                    ALTER TABLE ThanhToan ADD GhiChuQuyetToan NVARCHAR(500) NULL;
                END
            ");

            // 3. Bảng NguoiDung: Bổ sung TrangThaiDuyet và LyDoTuChoi
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'TrangThaiDuyet')
                BEGIN
                    ALTER TABLE NguoiDung ADD TrangThaiDuyet NVARCHAR(30) NULL CONSTRAINT DF_NguoiDung_TrangThaiDuyet DEFAULT 'HoatDong';
                END

                IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('NguoiDung') AND name = 'LyDoTuChoi')
                BEGIN
                    ALTER TABLE NguoiDung ADD LyDoTuChoi NVARCHAR(500) NULL;
                END
            ");

            // 4. Bảng DonDatPhong: Ràng buộc trạng thái
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_DonDatPhong_TrangThai')
                BEGIN
                    ALTER TABLE DonDatPhong ADD CONSTRAINT CK_DonDatPhong_TrangThai CHECK ([TrangThai] IN (N'ChoThanhToan', N'ChoDuyet', N'TuChoi', N'DaDuyet', N'DaHoanTat', N'YeuCauHoanTien', N'DaHoanTien', N'DaHuy'));
                END
            ");

            // 5. Cập nhật dữ liệu mẫu cho các mã giảm giá hiện có
            migrationBuilder.Sql(@"
                UPDATE GiamGia
                SET TenChuongTrinh = CASE 
                    WHEN TenMa = 'HE2026' THEN N'Chào hè rực rỡ 2026 - Giảm 15% tối đa 300K'
                    WHEN TenMa = 'CHAOBANMOI' THEN N'Ưu đãi thành viên mới - Giảm 20% tối đa 500K'
                    WHEN TenMa = 'TRAVELOKA10' THEN N'Đối tác du lịch liên kết - Giảm 10% tối đa 200K'
                    WHEN TenMa = 'STAYLYLUXURY' THEN N'Đặc quyền nghỉ dưỡng sang trọng - Giảm 12% tối đa 1 triệu'
                    WHEN TenMa = 'SUMMERVIBES' THEN N'Sắc màu mùa hè - Giảm 15% tối đa 400K'
                    ELSE N'Chương trình khuyến mãi Stayly'
                END,
                SoLuong = ISNULL(SoLuong, 100)
                WHERE TenChuongTrinh IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DonDatPhong_TrangThai",
                table: "DonDatPhong");

            migrationBuilder.DropColumn(
                name: "GhiChuQuyetToan",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "MaGiaoDichQuyetToan",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "NgayQuyetToan",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "SoTienQuyetToan",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "TrangThaiQuyetToan",
                table: "ThanhToan");

            migrationBuilder.DropColumn(
                name: "LyDoTuChoi",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "TrangThaiDuyet",
                table: "NguoiDung");

            migrationBuilder.DropColumn(
                name: "SoLuong",
                table: "GiamGia");

            migrationBuilder.DropColumn(
                name: "TenChuongTrinh",
                table: "GiamGia");
        }
    }
}
