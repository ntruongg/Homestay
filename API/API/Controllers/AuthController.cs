using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Models;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HomestayDbContext _context;

        public AuthController(HomestayDbContext context)
        {
            _context = context;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(TaiKhoan model)
        {
            if (await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == model.TenDangNhap))
            {
                return BadRequest("Tên đăng nhập đã tồn tại.");
            }

            _context.TaiKhoans.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công!", data = model });
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.TaiKhoans
                .FirstOrDefaultAsync(t => t.TenDangNhap == loginDto.TenDangNhap && t.MatKhau == loginDto.MatKhau);

            if (user == null)
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }

            // Trả về thông tin tài khoản và vai trò (VaiTro: OWNER hoặc GUEST)
            return Ok(new { message = "Đăng nhập thành công", user });
        }
    }

    // DTO phụ trợ nhận dữ liệu đăng nhập
    public class LoginDto
    {
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
    }
}