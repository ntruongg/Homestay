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
    public DbSet<LichSuDuyet> LichSuDuyets => Set<LichSuDuyet>();
    public DbSet<VaiTro> VaiTros => Set<VaiTro>();
    public DbSet<PhuThu> PhuThus => Set<PhuThu>();

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
            entity.HasOne(x => x.VaiTro).WithMany(x => x.TaiKhoans)
                .HasForeignKey(x => x.MaVaiTro).OnDelete(DeleteBehavior.Restrict);
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
            entity.Property(x => x.NgayBatDau).HasColumnType("date");
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
            entity.HasOne(x => x.DonDatPhong)
                .WithMany(x => x.ChiTietDons)
                .HasForeignKey(x => x.MaDonDatPhong)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Phong)
                .WithMany(x => x.ChiTietDons)
                .HasForeignKey(x => x.MaPhong)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne(x => x.Phong)
                .WithMany()
                .HasForeignKey(x => x.MaPhong)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TienNghi>().ToTable("TienNghi");
        modelBuilder.Entity<CoSoLuuTru_TienNghi>(entity =>
        {
            entity.ToTable("CoSoLuuTru_TienNghi");
            entity.HasKey(x => new { x.MaCoSoLuuTru, x.MaTienNghi });
            entity.HasOne(x => x.CoSoLuuTru)
                .WithMany(x => x.TienNghis)
                .HasForeignKey(x => x.MaCoSoLuuTru)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TienNghi)
                .WithMany()
                .HasForeignKey(x => x.MaTienNghi)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Phong_TienNghi>(entity =>
        {
            entity.ToTable("Phong_TienNghi");
            entity.HasKey(x => new { x.MaPhong, x.MaTienNghi });
            entity.HasOne(x => x.Phong)
                .WithMany(x => x.TienNghis)
                .HasForeignKey(x => x.MaPhong)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.TienNghi)
                .WithMany()
                .HasForeignKey(x => x.MaTienNghi)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HinhAnh>(entity =>
        {
            entity.ToTable("HinhAnh");
            entity.HasOne(x => x.CoSoLuuTru)
                .WithMany()
                .HasForeignKey(x => x.MaCoSoLuuTru)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Phong)
                .WithMany()
                .HasForeignKey(x => x.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
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

        modelBuilder.Entity<LichSuDuyet>(entity =>
        {
            entity.ToTable("LichSuDuyet");
            entity.HasKey(x => x.MaLichSu);
            entity.Property(x => x.TrangThaiDuyet).HasMaxLength(30);
            entity.Property(x => x.LyDoTuChoi).HasMaxLength(500);
            entity.Property(x => x.NgayDuyet).HasColumnType("datetime");
            entity.HasOne(x => x.CoSoLuuTru)
                .WithMany(x => x.LichSuDuyets)
                .HasForeignKey(x => x.MaCoSoLuuTru)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.NguoiDuyet)
                .WithMany()
                .HasForeignKey(x => x.MaNguoiDuyet)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.ToTable("VaiTro");
            entity.HasKey(x => x.MaVaiTro);
            entity.HasData(
                new VaiTro { MaVaiTro = 1, TenVaiTro = "GUEST", MoTa = "Traveler / Guest" },
                new VaiTro { MaVaiTro = 2, TenVaiTro = "OWNER", MoTa = "Homestay Host / Owner" },
                new VaiTro { MaVaiTro = 3, TenVaiTro = "ADMIN", MoTa = "System Administrator" }
            );
        });

        modelBuilder.Entity<PhuThu>(entity =>
        {
            entity.ToTable("PhuThu");
            entity.HasKey(x => x.MaPhuThu);
            entity.HasOne(x => x.DonDatPhong)
                .WithMany(x => x.PhuThus)
                .HasForeignKey(x => x.MaDonDatPhong)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}