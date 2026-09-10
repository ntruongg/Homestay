using API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace API.Data
{
    public class HomestayDbContext : DbContext
    {
        public HomestayDbContext(DbContextOptions<HomestayDbContext> options) : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<ChuCoSoLuuTru> ChuCoSoLuuTrus { get; set; }
        public DbSet<CoSoLuuTru> CoSoLuuTrus { get; set; }
        public DbSet<LoaiPhong> LoaiPhongs { get; set; }
        public DbSet<Phong> Phongs { get; set; }
        public DbSet<TienNghi> TienNghis { get; set; }
        public DbSet<CoSoLuuTru_TienNghi> CoSoLuuTru_TienNghis { get; set; }
        public DbSet<Phong_TienNghi> Phong_TienNghis { get; set; }
        public DbSet<DonDatPhong> DonDatPhongs { get; set; }
        public DbSet<LichLuuTru> LichLuuTrus { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }

        public DbSet<HinhAnh> HinhAnhs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DonDatPhong>()
                .HasOne(d => d.KhachHang)
                .WithMany()
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<DonDatPhong>()
                .HasOne(d => d.Phong)
                .WithMany()
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoSoLuuTru_TienNghi>()
                .HasKey(c => new { c.MaCoSoLuuTru, c.MaTienNghi });

            modelBuilder.Entity<Phong_TienNghi>()
                .HasKey(p => new { p.MaPhong, p.MaTienNghi });

            // Thiết lập Unique
            modelBuilder.Entity<TaiKhoan>().HasIndex(t => t.TenDangNhap).IsUnique();
            modelBuilder.Entity<TaiKhoan>().HasIndex(t => t.Phone).IsUnique();
            modelBuilder.Entity<TaiKhoan>().HasIndex(t => t.Email).IsUnique();
            modelBuilder.Entity<ChuCoSoLuuTru>().HasIndex(c => c.CCCD).IsUnique();

            // Unique ngày của lịch lưu trú để tránh 1 phòng bị đè 2 lịch cùng ngày
            modelBuilder.Entity<LichLuuTru>().HasIndex(l => new { l.MaPhong, l.Ngay }).IsUnique();
        }
    }
}