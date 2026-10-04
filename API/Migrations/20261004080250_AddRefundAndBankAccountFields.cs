using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddRefundAndBankAccountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DonDatPhong",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "ChoThanhToan",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDung_CCCD",
                table: "NguoiDung",
                column: "CCCD",
                unique: true,
                filter: "[CCCD] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NguoiDung_CCCD",
                table: "NguoiDung");

            migrationBuilder.AlterColumn<string>(
                name: "TrangThai",
                table: "DonDatPhong",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "ChoThanhToan");
        }
    }
}
