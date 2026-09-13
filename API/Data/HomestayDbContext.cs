using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class HomestayDbContext(DbContextOptions<HomestayDbContext> options) : DbContext(options)
{
    public DbSet<TaiKhoan> TaiKhoans => Set<TaiKhoan>();
    public DbSet<CoSoLuuTru> CoSoLuuTrus => Set<CoSoLuuTru>();
    public DbSet<LoaiPhong> LoaiPhongs => Set<LoaiPhong>();
    public DbSet<Phong> Phongs => Set<Phong>();
    public DbSet<GiamGia> GiamGias => Set<GiamGia>();
    public DbSet<DonDatPhong> DonDatPhongs => Set<DonDatPhong>();
    public DbSet<ChiTietDon> ChiTietDons => Set<ChiTietDon>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    public DbSet<LichLuuTru> LichLuuTrus => Set<LichLuuTru>();
    public DbSet<TienNghi> TienNghis => Set<TienNghi>();
    public DbSet<CoSoLuuTru_TienNghi> CoSoLuuTru_TienNghis => Set<CoSoLuuTru_TienNghi>();
    public DbSet<Phong_TienNghi> Phong_TienNghis => Set<Phong_TienNghi>();
    public DbSet<HinhAnh> HinhAnhs => Set<HinhAnh>();
    public DbSet<DanhGia> DanhGias => Set<DanhGia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.ToTable("TaiKhoan");
            entity.Property(x => x.DienThoai).HasColumnName("DienThoai");
            entity.Property(x => x.GioiTinh).HasColumnType("char(1)");
            entity.Property(x => x.NgaySinh).HasColumnType("date");
            entity.Property(x => x.NgayTao).HasColumnType("date");
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.DienThoai).IsUnique();
        });

        modelBuilder.Entity<CoSoLuuTru>(entity =>
        {
            entity.ToTable("CoSoLuuTru");
            entity.Property(x => x.GiayPhepKinhDoanhUrl).HasColumnName("GiayPhepKD_URL");
            entity.Property(x => x.GiayToPcccUrl).HasColumnName("GiayToPCCC_URL");
            entity.Property(x => x.GiayToAnttUrl).HasColumnName("GiayToANTT_URL");
            entity.HasOne(x => x.ChuCoSoLuuTru).WithMany(x => x.CoSoLuuTrus)
                .HasForeignKey(x => x.MaChuCoSoLuuTru).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoaiPhong>().ToTable("LoaiPhong");
        modelBuilder.Entity<Phong>(entity =>
        {
            entity.ToTable("Phong");
            entity.Property(x => x.GiaGoc).HasColumnType("decimal(12,2)");
        });

        modelBuilder.Entity<GiamGia>(entity =>
        {
            entity.ToTable("GiamGia");
            entity.Property(x => x.ToiDa).HasColumnType("decimal(12,2)");
            entity.Property(x => x.NgayHetHan).HasColumnType("date");
        });

        modelBuilder.Entity<DonDatPhong>(entity =>
        {
            entity.ToTable("DonDatPhong");
            entity.Property(x => x.NgayDat).HasColumnType("date");
            entity.Property(x => x.NgayDen).HasColumnType("date");
            entity.Property(x => x.NgayDi).HasColumnType("date");
            entity.HasOne(x => x.KhachHang).WithMany()
                .HasForeignKey(x => x.MaKhachHang).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.GiamGia).WithMany(x => x.DonDatPhongs)
                .HasForeignKey(x => x.MaGiamGia).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChiTietDon>(entity =>
        {
            entity.ToTable("ChiTietDon");
            entity.HasKey(x => new { x.MaDonDatPhong, x.MaPhong });
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.ToTable("ThanhToan");
            entity.HasOne(x => x.DonDatPhong).WithOne(x => x.ThanhToan)
                .HasForeignKey<ThanhToan>(x => x.MaHoaDon).OnDelete(DeleteBehavior.Restrict);
            entity.Property(x => x.TongTien).HasColumnType("decimal(12,2)");
            entity.Property(x => x.TienGoc).HasColumnType("decimal(12,2)");
        });

        modelBuilder.Entity<LichLuuTru>(entity =>
        {
            entity.ToTable("LichLuuTru");
            entity.Property(x => x.Ngay).HasColumnType("date");
            entity.HasIndex(x => new { x.MaPhong, x.Ngay }).IsUnique();
        });

        modelBuilder.Entity<TienNghi>().ToTable("TienNghi");
        modelBuilder.Entity<CoSoLuuTru_TienNghi>(entity =>
        {
            entity.ToTable("CoSoLuuTru_TienNghi");
            entity.HasKey(x => new { x.MaCoSoLuuTru, x.MaTienNghi });
        });
        modelBuilder.Entity<Phong_TienNghi>(entity =>
        {
            entity.ToTable("Phong_TienNghi");
            entity.HasKey(x => new { x.MaPhong, x.MaTienNghi });
        });

        modelBuilder.Entity<HinhAnh>(entity => entity.ToTable("HinhAnh"));
        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.ToTable("DanhGia");

            entity.HasKey(x => x.MaDanhGia);

            entity.HasOne(x => x.DonDatPhong)
                .WithOne(x => x.DanhGia)
                .HasForeignKey<DanhGia>(x => x.MaDonDatPhong)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.MaDonDatPhong)
                .IsUnique();
        });
    }
}