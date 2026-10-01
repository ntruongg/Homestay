using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Migrations
{
    /// <inheritdoc />
    public partial class AddExclusiveOwnerConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_HinhAnh_ExclusiveOwner",
                table: "HinhAnh",
                sql: "(MaCoSoLuuTru IS NOT NULL AND MaPhong IS NULL) OR (MaCoSoLuuTru IS NULL AND MaPhong IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_HinhAnh_ExclusiveOwner",
                table: "HinhAnh");
        }
    }
}
