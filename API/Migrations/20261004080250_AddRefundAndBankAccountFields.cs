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

            migrationBuilder.Sql(@"
                DECLARE @constraintName NVARCHAR(200);
                SELECT @constraintName = kc.name
                FROM sys.key_constraints kc
                JOIN sys.index_columns ic ON kc.parent_object_id = ic.object_id AND kc.unique_index_id = ic.index_id
                JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                WHERE kc.parent_object_id = OBJECT_ID('NguoiDung') AND kc.type = 'UQ' AND c.name = 'CCCD';

                IF @constraintName IS NOT NULL
                BEGIN
                    EXEC('ALTER TABLE NguoiDung DROP CONSTRAINT [' + @constraintName + ']');
                END
            ");

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
