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
    public class KhachHangsController : ControllerBase
    {
        private readonly HomestayDbContext _context;

        public KhachHangsController(HomestayDbContext context)
        {
            _context = context;
        }

        // GET: api/KhachHangs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<KhachHang>>> GetKhachHangs()
        {
            return await _context.KhachHangs
                .Include(k => k.MaKhachHangNavigation) // Lấy kèm thông tin tài khoản chung
                .ToListAsync();
        }

        // GET: api/KhachHangs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<KhachHang>> GetKhachHang(int id)
        {
            var khachHang = await _context.KhachHangs
                .Include(k => k.MaKhachHangNavigation)
                .FirstOrDefaultAsync(k => k.MaKhachHang == id);

            if (khachHang == null)
            {
                return NotFound("Không tìm thấy thông tin khách hàng.");
            }

            return khachHang;
        }

        // POST: api/KhachHangs
        [HttpPost]
        public async Task<ActionResult<KhachHang>> PostKhachHang(KhachHang khachHang)
        {
            _context.KhachHangs.Add(khachHang);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetKhachHang), new { id = khachHang.MaKhachHang }, khachHang);
        }

        // PUT: api/KhachHangs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutKhachHang(int id, KhachHang khachHang)
        {
            if (id != khachHang.MaKhachHang)
            {
                return BadRequest("Mã khách hàng không khớp.");
            }

            _context.Entry(khachHang).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KhachHangExists(id))
                {
                    return NotFound("Không tìm thấy khách hàng để cập nhật.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/KhachHangs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteKhachHang(int id)
        {
            var khachHang = await _context.KhachHangs.FindAsync(id);
            if (khachHang == null)
            {
                return NotFound("Không tìm thấy khách hàng để xóa.");
            }

            _context.KhachHangs.Remove(khachHang);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool KhachHangExists(int id)
        {
            return _context.KhachHangs.Any(e => e.MaKhachHang == id);
        }
    }
}