using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class HomestayDbContext(DbContextOptions<HomestayDbContext> options) : DbContext(options)
{
    public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<CoSoLuuTru> CoSoLuuTrus => Set<CoSoLuuTru>();
    public DbSet<LoaiPhong> LoaiPhongs => Set<LoaiPhong>();
    public DbSet<Phong> Phongs => Set<Phong>();
    public DbSet<TienNghiCoSo> TienNghiCoSos => Set<TienNghiCoSo>();
    public DbSet<TienNghiPhong> TienNghiPhongs => Set<TienNghiPhong>();
    public DbSet<CoSoLuuTru_TienNghi> CoSoLuuTru_TienNghis => Set<CoSoLuuTru_TienNghi>();
    public DbSet<Phong_TienNghi> Phong_TienNghis => Set<Phong_TienNghi>();
    public DbSet<HinhAnh> HinhAnhs => Set<HinhAnh>();
    public DbSet<GiamGia> GiamGias => Set<GiamGia>();
    public DbSet<DonDatPhong> DonDatPhongs => Set<DonDatPhong>();
    public DbSet<ChiTietDon> ChiTietDons => Set<ChiTietDon>();
    public DbSet<DichVu> DichVus => Set<DichVu>();
    public DbSet<DonDatPhongDichVu> DonDatPhongDichVus => Set<DonDatPhongDichVu>();
    public DbSet<ThanhToan> ThanhToans => Set<ThanhToan>();
    public DbSet<LichLuuTru> LichLuuTrus => Set<LichLuuTru>();
    public DbSet<DanhGia> DanhGias => Set<DanhGia>();
    public DbSet<LichSuDuyet> LichSuDuyets => Set<LichSuDuyet>();
    public DbSet<NhatKyHoatDong> NhatKyHoatDongs => Set<NhatKyHoatDong>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.ToTable("VaiTro");
            entity.HasKey(x => x.MaVaiTro);
            entity.HasIndex(x => x.TenVaiTro).IsUnique();
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.ToTable("NguoiDung");
            entity.HasKey(x => x.MaNguoiDung);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.DienThoai).IsUnique();
            entity.Property(x => x.GioiTinh).HasColumnType("char(1)");
            entity.Property(x => x.NgaySinh).HasColumnType("date");
            entity.Property(x => x.NgayTao).HasColumnType("datetime");
            entity.HasOne(x => x.VaiTro).WithMany(x => x.NguoiDungs)
                .HasForeignKey(x => x.MaVaiTro).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CoSoLuuTru>(entity =>
        {
            entity.ToTable("CoSoLuuTru");
            entity.HasKey(x => x.MaCoSoLuuTru);
            entity.Property(x => x.GiayPhepKinhDoanhUrl).HasColumnName("GiayPhepKD_URL");
            entity.Property(x => x.GiayToPcccUrl).HasColumnName("GiayToPCCC_URL");
            entity.Property(x => x.GiayToAnttUrl).HasColumnName("GiayToANTT_URL");
            entity.HasOne(x => x.ChuCoSoLuuTru).WithMany(x => x.CoSoLuuTrus)
                .HasForeignKey(x => x.MaChuCoSoLuuTru).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoaiPhong>(entity =>
        {
            entity.ToTable("LoaiPhong");
            entity.HasKey(x => x.MaLoaiPhong);
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.ToTable("Phong");
            entity.HasKey(x => x.MaPhong);
            entity.Property(x => x.GiaGoc).HasColumnType("decimal(12,2)");
            entity.HasOne(x => x.CoSoLuuTru).WithMany(x => x.Phongs)
                .HasForeignKey(x => x.MaCoSoLuuTru).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.LoaiPhong).WithMany()
                .HasForeignKey(x => x.MaLoaiPhong).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TienNghiCoSo>(entity =>
        {
            entity.ToTable("TienNghiCoSo");
            entity.HasKey(x => x.MaTienNghi);
            entity.HasIndex(x => x.TenTienNghi).IsUnique();
        });

        modelBuilder.Entity<TienNghiPhong>(entity =>
        {
            entity.ToTable("TienNghiPhong");
            entity.HasKey(x => x.MaTienNghi);
            entity.HasIndex(x => x.TenTienNghi).IsUnique();
        });

        modelBuilder.Entity<CoSoLuuTru_TienNghi>(entity =>
        {
            entity.ToTable("CoSoLuuTru_TienNghi");
            entity.HasKey(x => new { x.MaCoSoLuuTru, x.MaTienNghi });
            entity.HasOne(x => x.CoSoLuuTru).WithMany(x => x.TienNghis)
                .HasForeignKey(x => x.MaCoSoLuuTru).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TienNghiCoSo).WithMany()
                .HasForeignKey(x => x.MaTienNghi).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Phong_TienNghi>(entity =>
        {
            entity.ToTable("Phong_TienNghi");
            entity.HasKey(x => new { x.MaPhong, x.MaTienNghi });
            entity.HasOne(x => x.Phong).WithMany(x => x.TienNghis)
                .HasForeignKey(x => x.MaPhong).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TienNghiPhong).WithMany()
                .HasForeignKey(x => x.MaTienNghi).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HinhAnh>(entity =>
        {
            entity.ToTable("HinhAnh");
            entity.HasKey(x => x.MaHinhAnh);
            entity.HasOne(x => x.CoSoLuuTru).WithMany(x => x.HinhAnhs)
                .HasForeignKey(x => x.MaCoSoLuuTru).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Phong).WithMany(x => x.HinhAnhs)
                .HasForeignKey(x => x.MaPhong).OnDelete(DeleteBehavior.ClientSetNull);

            // Ràng buộc 1: Exclusive Arc - Ảnh phải thuộc về Cơ sở HOẶC Phòng (Không được cả 2, không được bỏ trống cả 2)
            entity.ToTable(t => t.HasCheckConstraint("CK_HinhAnh_ExclusiveOwner",
                "(MaCoSoLuuTru IS NOT NULL AND MaPhong IS NULL) OR (MaCoSoLuuTru IS NULL AND MaPhong IS NOT NULL)"));
        });

        modelBuilder.Entity<GiamGia>(entity =>
        {
            entity.ToTable("GiamGia");
            entity.HasKey(x => x.MaGiamGia);
            entity.HasIndex(x => x.TenMa).IsUnique();
            entity.Property(x => x.ToiDa).HasColumnType("decimal(12,2)");
            entity.Property(x => x.NgayBatDau).HasColumnType("date");
            entity.Property(x => x.NgayHetHan).HasColumnType("date");
        });

        modelBuilder.Entity<DonDatPhong>(entity =>
        {
            entity.ToTable("DonDatPhong");
            entity.HasKey(x => x.MaDonDatPhong);
            entity.Property(x => x.NgayDat).HasColumnType("datetime");
            entity.Property(x => x.NgayDen).HasColumnType("date");
            entity.Property(x => x.NgayDi).HasColumnType("date");
            entity.HasOne(x => x.KhachHang).WithMany(x => x.DonDatPhongs)
                .HasForeignKey(x => x.MaKhachHang).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.GiamGia).WithMany(x => x.DonDatPhongs)
                .HasForeignKey(x => x.MaGiamGia).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChiTietDon>(entity =>
        {
            entity.ToTable("ChiTietDon");
            entity.HasKey(x => new { x.MaDonDatPhong, x.MaPhong });
            entity.Property(x => x.DonGia).HasColumnType("decimal(12,2)");
            entity.HasOne(x => x.DonDatPhong).WithMany(x => x.ChiTietDons)
                .HasForeignKey(x => x.MaDonDatPhong).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Phong).WithMany(x => x.ChiTietDons)
                .HasForeignKey(x => x.MaPhong).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DichVu>(entity =>
        {
            entity.ToTable("DichVu");
            entity.HasKey(x => x.MaDichVu);
            entity.Property(x => x.GiaDichVu).HasColumnType("decimal(12,2)");
            entity.HasOne(x => x.CoSoLuuTru).WithMany(x => x.DichVus)
                .HasForeignKey(x => x.MaCoSoLuuTru).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DonDatPhongDichVu>(entity =>
        {
            entity.ToTable("DonDatPhongDichVu");
            entity.HasKey(x => new { x.MaDonDatPhong, x.MaDichVu });
            entity.Property(x => x.DonGia).HasColumnType("decimal(12,2)");
            entity.Property(x => x.ThanhTien).HasColumnType("decimal(12,2)");

            entity.HasOne(x => x.DonDatPhong).WithMany(x => x.DonDatPhongDichVus)
                .HasForeignKey(x => x.MaDonDatPhong).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.DichVu).WithMany(x => x.DonDatPhongDichVus)
                .HasForeignKey(x => x.MaDichVu).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.ToTable("ThanhToan");
            entity.HasKey(x => x.MaHoaDon);
            entity.Property(x => x.TongTien).HasColumnType("decimal(12,2)");
            entity.Property(x => x.TienGoc).HasColumnType("decimal(12,2)");
            entity.Property(x => x.PhanTramHoaHong).HasColumnType("decimal(5,2)");
            entity.Property(x => x.TienHoaHong).HasColumnType("decimal(12,2)");
            entity.Property(x => x.TienThucNhanChu).HasColumnType("decimal(12,2)");
            entity.Property(x => x.NgayThanhToan).HasColumnType("datetime");
            entity.HasOne(x => x.DonDatPhong).WithOne(x => x.ThanhToan)
                .HasForeignKey<ThanhToan>(x => x.MaHoaDon).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LichLuuTru>(entity =>
        {
            entity.ToTable("LichLuuTru");
            entity.HasKey(x => x.MaLich);
            entity.Property(x => x.Ngay).HasColumnType("date");
            entity.HasIndex(x => new { x.MaPhong, x.Ngay }).IsUnique();
            entity.HasOne(x => x.Phong).WithMany()
                .HasForeignKey(x => x.MaPhong).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.ToTable("DanhGia");
            entity.HasKey(x => x.MaDanhGia);
            entity.HasIndex(x => x.MaDonDatPhong).IsUnique();
            entity.Property(x => x.NgayDanhGia).HasColumnType("datetime");
            entity.Property(x => x.NgayPhanHoi).HasColumnType("datetime");
            entity.HasOne(x => x.DonDatPhong).WithOne(x => x.DanhGia)
                .HasForeignKey<DanhGia>(x => x.MaDonDatPhong).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LichSuDuyet>(entity =>
        {
            entity.ToTable("LichSuDuyet");
            entity.HasKey(x => x.MaLichSu);
            entity.Property(x => x.NgayDuyet).HasColumnType("datetime");
            entity.HasOne(x => x.CoSoLuuTru).WithMany(x => x.LichSuDuyets)
                .HasForeignKey(x => x.MaCoSoLuuTru).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.NguoiDuyet).WithMany(x => x.LichSuDuyets)
                .HasForeignKey(x => x.MaNguoiDuyet).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<NhatKyHoatDong>(entity =>
        {
            entity.ToTable("NhatKyHoatDong");
            entity.HasKey(x => x.MaNhatKy);
            entity.Property(x => x.ThoiGian).HasColumnType("datetime");
            entity.HasIndex(x => x.ThoiGian);
            entity.HasOne(x => x.NguoiDung).WithMany(x => x.NhatKyHoatDongs)
                .HasForeignKey(x => x.MaNguoiDung).OnDelete(DeleteBehavior.SetNull);
        });
    }
}