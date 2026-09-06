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
    public class ChuCoSoLuuTrusController : ControllerBase
    {
        private readonly HomestayDbContext _context;

        public ChuCoSoLuuTrusController(HomestayDbContext context)
        {
            _context = context;
        }

        // GET: api/ChuCoSoLuuTrus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChuCoSoLuuTru>>> GetChuCoSoLuuTrus()
        {
            return await _context.ChuCoSoLuuTrus
                .Include(c => c.MaChuCoSoLuuTruNavigation) // Lấy kèm thông tin tài khoản
                .ToListAsync();
        }

        // GET: api/ChuCoSoLuuTrus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ChuCoSoLuuTru>> GetChuCoSoLuuTru(int id)
        {
            var chuCoSo = await _context.ChuCoSoLuuTrus
                .Include(c => c.MaChuCoSoLuuTruNavigation)
                .Include(c => c.CoSoLuuTrus) // Lấy kèm danh sách cơ sở lưu trú của chủ này
                .FirstOrDefaultAsync(c => c.MaChuCoSoLuuTru == id);

            if (chuCoSo == null)
            {
                return NotFound("Không tìm thấy chủ cơ sở lưu trú.");
            }

            return chuCoSo;
        }

        // POST: api/ChuCoSoLuuTrus
        [HttpPost]
        public async Task<ActionResult<ChuCoSoLuuTru>> PostChuCoSoLuuTru(ChuCoSoLuuTru chuCoSoLuuTru)
        {
            _context.ChuCoSoLuuTrus.Add(chuCoSoLuuTru);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChuCoSoLuuTru), new { id = chuCoSoLuuTru.MaChuCoSoLuuTru }, chuCoSoLuuTru);
        }

        // PUT: api/ChuCoSoLuuTrus/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChuCoSoLuuTru(int id, ChuCoSoLuuTru chuCoSoLuuTru)
        {
            if (id != chuCoSoLuuTru.MaChuCoSoLuuTru)
            {
                return BadRequest("Mã chủ cơ sở lưu trú không khớp.");
            }

            _context.Entry(chuCoSoLuuTru).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChuCoSoLuuTruExists(id))
                {
                    return NotFound("Không tìm thấy chủ cơ sở để cập nhật.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ChuCoSoLuuTrus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChuCoSoLuuTru(int id)
        {
            var chuCoSo = await _context.ChuCoSoLuuTrus.FindAsync(id);
            if (chuCoSo == null)
            {
                return NotFound("Không tìm thấy chủ cơ sở để xóa.");
            }

            _context.ChuCoSoLuuTrus.Remove(chuCoSo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ChuCoSoLuuTruExists(int id)
        {
            return _context.ChuCoSoLuuTrus.Any(e => e.MaChuCoSoLuuTru == id);
        }
    }
}