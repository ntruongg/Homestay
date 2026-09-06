using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DanhGiasController : ControllerBase
    {
        private readonly HomestayDbContext _context;

        public DanhGiasController(HomestayDbContext context)
        {
            _context = context;
        }

        // GET: api/DanhGias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DanhGium>>> GetDanhGia()
        {
            return await _context.DanhGia.ToListAsync();
        }

        // POST: api/DanhGias (Khách hàng gửi đánh giá cho đơn đặt phòng)
        [HttpPost]
        public async Task<ActionResult<DanhGium>> PostDanhGia(DanhGium danhGia)
        {
            // Kiểm tra xem đơn đặt phòng này đã được đánh giá trước đó chưa (vì quy định 1 đơn chỉ 1 đánh giá)
            bool daDanhGia = await _context.DanhGia.AnyAsync(d => d.MaDonDatPhong == danhGia.MaDonDatPhong);
            if (daDanhGia)
            {
                return BadRequest("Đơn đặt phòng này đã có đánh giá rồi.");
            }

            _context.DanhGia.Add(danhGia);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDanhGia), new { id = danhGia.MaDanhGia }, danhGia);
        }
    }
}