using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonDatPhongController : ControllerBase
    {
        private readonly HomestayDbContext _context;

        public DonDatPhongController(HomestayDbContext context)
        {
            _context = context;
        }

        // GET: api/DonDatPhongs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonDatPhong>>> GetDonDatPhongs()
        {
            return await _context.DonDatPhongs
                .Include(d => d.MaKhachHangNavigation)
                .Include(d => d.MaPhongNavigation)
                .Include(d => d.MaCoSoLuuTruNavigation)
                .ToListAsync();
        }

        // POST: api/DonDatPhongs (Tạo đơn đặt phòng mới)
        [HttpPost]
        public async Task<ActionResult<DonDatPhong>> PostDonDatPhong(DonDatPhong donDatPhong)
        {
            // Kiểm tra ràng buộc: Chỉ được chọn hoặc Homestay (nguyên căn) hoặc Phòng lẻ, không được chọn cả hai hoặc bỏ trống cả hai
            if ((donDatPhong.MaCoSoLuuTru != null && donDatPhong.MaPhong != null) ||
                (donDatPhong.MaCoSoLuuTru == null && donDatPhong.MaPhong == null))
            {
                return BadRequest("Đơn đặt phòng phải chọn hoặc nguyên căn (MaCoSoLuuTru) hoặc phòng lẻ (MaPhong), không được chọn đồng thời.");
            }

            donDatPhong.TrangThai = "Pending"; // Mặc định trạng thái chờ xác nhận
            _context.DonDatPhongs.Add(donDatPhong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDonDatPhongs), new { id = donDatPhong.MaDonDatPhong }, donDatPhong);
        }

        // PUT: api/DonDatPhongs/5/status (Cập nhật trạng thái đơn: Confirmed, Cancelled, CheckedIn...)
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTrangThai(int id, [FromBody] string trangThaiMoi)
        {
            var donDat = await _context.DonDatPhongs.FindAsync(id);
            if (donDat == null)
            {
                return NotFound("Không tìm thấy đơn đặt phòng.");
            }

            donDat.TrangThai = trangThaiMoi;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}