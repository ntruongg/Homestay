using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhongsController : ControllerBase
    {
        private readonly HomestayDbContext _context;

        public PhongsController(HomestayDbContext context)
        {
            _context = context;
        }

        private bool PhongExists(int id)
        {
            return _context.Phongs.Any(e => e.MaPhong == id);
        }

        // GET: api/Phongs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Phong>>> GetPhongs()
        {
            return await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation) // Lấy kèm thông tin loại phòng
                .ToListAsync();
        }

        // GET: api/Phongs/Cosoluutru/5 (Lấy tất cả phòng của một homestay cụ thể)
        [HttpGet("cosoluutru/{maCoSoLuuTru}")]
        public async Task<ActionResult<IEnumerable<Phong>>> GetPhongsByCoSo(int maCoSoLuuTru)
        {
            return await _context.Phongs
                .Where(p => p.MaCoSoLuuTru == maCoSoLuuTru)
                .Include(p => p.MaLoaiPhongNavigation)
                .ToListAsync();
        }

        // GET: api/Phongs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Phong>> GetPhong(int id)
        {
            var phong = await _context.Phongs
                .Include(p => p.MaLoaiPhongNavigation)
                .FirstOrDefaultAsync(p => p.MaPhong == id);

            if (phong == null)
            {
                return NotFound("Không tìm thấy phòng.");
            }

            return phong;
        }

        // POST: api/Phongs
        [HttpPost]
        public async Task<ActionResult<Phong>> PostPhong(Phong phong)
        {
            _context.Phongs.Add(phong);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhong), new { id = phong.MaPhong }, phong);
        }

        // PUT: api/Phongs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPhong(int id, Phong phong)
        {
            if (id != phong.MaPhong)
            {
                return BadRequest("Mã phòng không khớp.");
            }

            _context.Entry(phong).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhongExists(id))
                {
                    return NotFound("Không tìm thấy phòng để cập nhật.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Phongs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhong(int id)
        {
            var phong = await _context.Phongs.FindAsync(id);
            if (phong == null)
            {
                return NotFound("Không tìm thấy phòng để xóa.");
            }

            _context.Phongs.Remove(phong);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        
    }
}