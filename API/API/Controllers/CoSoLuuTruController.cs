using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoSoLuuTruController : ControllerBase
    {
        private readonly HomestayDbContext _context;
        public CoSoLuuTruController(HomestayDbContext context)
        {
            _context = context;
        }

        private bool CoSoLuuTruExists(int id)
        {
            return _context.CoSoLuuTrus.Any(e => e.MaCoSoLuuTru == id);
        }
        // GET: api/<CoSoLuuTruController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CoSoLuuTru>>> GetAllCoSoLuuTru()
        {
            return await _context.CoSoLuuTrus.Include(c => c.MaChuCoSoLuuTruNavigation).ToListAsync();
        }

        // GET api/<CoSoLuuTruController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CoSoLuuTru>> GetCoSoLuuTru(int id)
        {
            var coSoLuuTru = await _context.CoSoLuuTrus.Include(c => c.Phongs).FirstOrDefaultAsync(c => c.MaCoSoLuuTru == id);

            if (coSoLuuTru == null) return NotFound();
            return coSoLuuTru;
        }

        // POST api/<CoSoLuuTruController>
        [HttpPost]
        public async Task<ActionResult<CoSoLuuTru>> PostCoSoLuuTru(CoSoLuuTru coSoLuuTru)
        {
            _context.CoSoLuuTrus.Add(coSoLuuTru);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCoSoLuuTru), new { id = coSoLuuTru.MaCoSoLuuTru }, coSoLuuTru);    
        }

        // PUT api/<CoSoLuuTruController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCoSoLuuTru(int id, CoSoLuuTru coSoLuuTru)
        {
            if (id != coSoLuuTru.MaCoSoLuuTru) return BadRequest();

            _context.Entry(coSoLuuTru).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CoSoLuuTruExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }


        // DELETE api/<CoSoLuuTruController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoSoLuuTru(int id)
        {
            var coSoLuuTru = await _context.CoSoLuuTrus.FindAsync(id);
            if (coSoLuuTru == null) return NotFound();

            _context.CoSoLuuTrus.Remove(coSoLuuTru);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
