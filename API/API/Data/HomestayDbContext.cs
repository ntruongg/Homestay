using System;
using System.Collections.Generic;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public partial class HomestayDbContext : DbContext
{
    public HomestayDbContext()
    {
    }

    public HomestayDbContext(DbContextOptions<HomestayDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

    public virtual DbSet<ChuCoSoLuuTru> ChuCoSoLuuTrus { get; set; }

    public virtual DbSet<CoSoLuuTru> CoSoLuuTrus { get; set; }

    public virtual DbSet<DanhGium> DanhGia { get; set; }

    public virtual DbSet<DonDatPhong> DonDatPhongs { get; set; }

    public virtual DbSet<HinhAnh> HinhAnhs { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<LichLuuTru> LichLuuTrus { get; set; }

    public virtual DbSet<LoaiPhong> LoaiPhongs { get; set; }

    public virtual DbSet<Phong> Phongs { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<TienNghi> TienNghis { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.MaChiTiet).HasName("PK__ChiTietH__CDF0A114A3164D4E");

            entity.ToTable("ChiTietHoaDon");

            entity.Property(e => e.DonGia).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.ThanhTien)
                .HasComputedColumnSql("([DonGia]*[SoLuong])", false)
                .HasColumnType("decimal(23, 2)");

            entity.HasOne(d => d.MaHoaDonNavigation).WithMany(p => p.ChiTietHoaDons)
                .HasForeignKey(d => d.MaHoaDon)
                .HasConstraintName("FK__ChiTietHo__MaHoa__5FB337D6");
        });

        modelBuilder.Entity<ChuCoSoLuuTru>(entity =>
        {
            entity.HasKey(e => e.MaChuCoSoLuuTru).HasName("PK__ChuCoSoL__BA45510BE67FAC15");

            entity.ToTable("ChuCoSoLuuTru");

            entity.HasIndex(e => e.Cccd, "UQ__ChuCoSoL__A955A0AAB7449204").IsUnique();

            entity.Property(e => e.MaChuCoSoLuuTru).ValueGeneratedNever();
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.ThongTinNganHang).HasMaxLength(100);

            entity.HasOne(d => d.MaChuCoSoLuuTruNavigation).WithOne(p => p.ChuCoSoLuuTru)
                .HasForeignKey<ChuCoSoLuuTru>(d => d.MaChuCoSoLuuTru)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChuCoSoLu__MaChu__440B1D61");
        });

        modelBuilder.Entity<CoSoLuuTru>(entity =>
        {
            entity.HasKey(e => e.MaCoSoLuuTru).HasName("PK__CoSoLuuT__ED7C1678F326FE9E");

            entity.ToTable("CoSoLuuTru");

            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.DienThoai).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GiayPhepKdUrl)
                .HasMaxLength(200)
                .HasColumnName("GiayPhepKD_URL");
            entity.Property(e => e.GiayToAnttUrl)
                .HasMaxLength(200)
                .HasColumnName("GiayToANTT_URL");
            entity.Property(e => e.GiayToPcccUrl)
                .HasMaxLength(200)
                .HasColumnName("GiayToPCCC_URL");
            entity.Property(e => e.LoaiHinh)
                .HasMaxLength(50)
                .HasDefaultValue("Homestay");
            entity.Property(e => e.LyDoTuChoi).HasMaxLength(200);
            entity.Property(e => e.TenCoSoLuuTru).HasMaxLength(100);
            entity.Property(e => e.TrangThaiDuyet).HasMaxLength(20);

            entity.HasOne(d => d.MaChuCoSoLuuTruNavigation).WithMany(p => p.CoSoLuuTrus)
                .HasForeignKey(d => d.MaChuCoSoLuuTru)
                .HasConstraintName("FK__CoSoLuuTr__MaChu__46E78A0C");

            entity.HasMany(d => d.MaTienNghis).WithMany(p => p.MaCoSoLuuTrus)
                .UsingEntity<Dictionary<string, object>>(
                    "CoSoLuuTruTienNghi",
                    r => r.HasOne<TienNghi>().WithMany()
                        .HasForeignKey("MaTienNghi")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__CoSoLuuTr__MaTie__6E01572D"),
                    l => l.HasOne<CoSoLuuTru>().WithMany()
                        .HasForeignKey("MaCoSoLuuTru")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__CoSoLuuTr__MaCoS__6D0D32F4"),
                    j =>
                    {
                        j.HasKey("MaCoSoLuuTru", "MaTienNghi").HasName("PK__CoSoLuuT__23ABAE8CC4C28605");
                        j.ToTable("CoSoLuuTru_TienNghi");
                    });
        });

        modelBuilder.Entity<DanhGium>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGia__AA9515BF4288ACD0");

            entity.HasIndex(e => e.MaDonDatPhong, "UQ__DanhGia__F7BDF64622A9B2B1").IsUnique();

            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaDonDatPhongNavigation).WithOne(p => p.DanhGium)
                .HasForeignKey<DanhGium>(d => d.MaDonDatPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DanhGia__MaDonDa__797309D9");
        });

        modelBuilder.Entity<DonDatPhong>(entity =>
        {
            entity.HasKey(e => e.MaDonDatPhong).HasName("PK__DonDatPh__F7BDF64703B8AFAB");

            entity.ToTable("DonDatPhong");

            entity.Property(e => e.NgayDat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TongTien).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(30);

            entity.HasOne(d => d.MaCoSoLuuTruNavigation).WithMany(p => p.DonDatPhongs)
                .HasForeignKey(d => d.MaCoSoLuuTru)
                .HasConstraintName("FK__DonDatPho__MaCoS__5441852A");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.DonDatPhongs)
                .HasForeignKey(d => d.MaKhachHang)
                .HasConstraintName("FK__DonDatPho__MaKha__534D60F1");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.DonDatPhongs)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__DonDatPho__MaPho__5535A963");
        });

        modelBuilder.Entity<HinhAnh>(entity =>
        {
            entity.HasKey(e => e.MaHinhAnh).HasName("PK__HinhAnh__A9C37A9BF11BCA99");

            entity.ToTable("HinhAnh");

            entity.HasOne(d => d.MaCoSoLuuTruNavigation).WithMany(p => p.HinhAnhs)
                .HasForeignKey(d => d.MaCoSoLuuTru)
                .HasConstraintName("FK__HinhAnh__MaCoSoL__74AE54BC");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.HinhAnhs)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__HinhAnh__MaPhong__75A278F5");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HoaDon__835ED13B867BCACF");

            entity.ToTable("HoaDon");

            entity.Property(e => e.NgayLap).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(30);
            entity.Property(e => e.TongTien).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.MaDonDatPhongNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaDonDatPhong)
                .HasConstraintName("FK__HoaDon__MaDonDat__5BE2A6F2");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KhachHan__88D2F0E54E69A6E4");

            entity.ToTable("KhachHang");

            entity.Property(e => e.MaKhachHang).ValueGeneratedNever();
            entity.Property(e => e.DiaChi).HasMaxLength(200);

            entity.HasOne(d => d.MaKhachHangNavigation).WithOne(p => p.KhachHang)
                .HasForeignKey<KhachHang>(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__KhachHang__MaKha__403A8C7D");
        });

        modelBuilder.Entity<LichLuuTru>(entity =>
        {
            entity.HasKey(e => e.MaLich).HasName("PK__LichLuuT__728A9AE9008A3495");

            entity.ToTable("LichLuuTru");

            entity.HasIndex(e => new { e.MaCoSoLuuTru, e.Ngay }, "UQ_LichHomestay_Ngay").IsUnique();

            entity.HasIndex(e => new { e.MaPhong, e.Ngay }, "UQ_LichPhong_Ngay").IsUnique();

            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Trống");

            entity.HasOne(d => d.MaCoSoLuuTruNavigation).WithMany(p => p.LichLuuTrus)
                .HasForeignKey(d => d.MaCoSoLuuTru)
                .HasConstraintName("FK__LichLuuTr__MaCoS__6477ECF3");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.LichLuuTrus)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__LichLuuTr__MaPho__656C112C");
        });

        modelBuilder.Entity<LoaiPhong>(entity =>
        {
            entity.HasKey(e => e.MaLoaiPhong).HasName("PK__LoaiPhon__2302121759892A37");

            entity.ToTable("LoaiPhong");

            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.TenLoaiPhong).HasMaxLength(50);
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__Phong__20BD5E5BE22A6455");

            entity.ToTable("Phong");

            entity.Property(e => e.GiaHienTai).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.SoPhong).HasMaxLength(10);
            entity.Property(e => e.TinhTrang).HasMaxLength(30);

            entity.HasOne(d => d.MaCoSoLuuTruNavigation).WithMany(p => p.Phongs)
                .HasForeignKey(d => d.MaCoSoLuuTru)
                .HasConstraintName("FK__Phong__MaCoSoLuu__4D94879B");

            entity.HasOne(d => d.MaLoaiPhongNavigation).WithMany(p => p.Phongs)
                .HasForeignKey(d => d.MaLoaiPhong)
                .HasConstraintName("FK__Phong__MaLoaiPho__4F7CD00D");

            entity.HasMany(d => d.MaTienNghis).WithMany(p => p.MaPhongs)
                .UsingEntity<Dictionary<string, object>>(
                    "PhongTienNghi",
                    r => r.HasOne<TienNghi>().WithMany()
                        .HasForeignKey("MaTienNghi")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Phong_Tie__MaTie__71D1E811"),
                    l => l.HasOne<Phong>().WithMany()
                        .HasForeignKey("MaPhong")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__Phong_Tie__MaPho__70DDC3D8"),
                    j =>
                    {
                        j.HasKey("MaPhong", "MaTienNghi").HasName("PK__Phong_Ti__EE6AE6AF68B697AC");
                        j.ToTable("Phong_TienNghi");
                    });
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C6529A22F8D9F");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0651FA140").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ__TaiKhoan__5C7E359E06BB0A97").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__TaiKhoan__A9D105348E21A727").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.GioiTinh)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau).HasMaxLength(100);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap).HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasDefaultValue(true);
            entity.Property(e => e.VaiTro).HasMaxLength(30);
        });

        modelBuilder.Entity<TienNghi>(entity =>
        {
            entity.HasKey(e => e.MaTienNghi).HasName("PK__TienNghi__ED7B8F4D6A96D1D7");

            entity.ToTable("TienNghi");

            entity.Property(e => e.TenTienNghi).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
