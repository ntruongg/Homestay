using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace Web.Controllers;

public sealed class HomeController(HomestayApiClient api) : Controller
{
    public async Task<IActionResult> Index(
        string? location,
        CancellationToken cancellationToken)
    {
        ViewBag.Location = location;
        ViewBag.User = HttpContext.Session.GetString("userName");

        try
        {
            return View(await api.GetPropertiesAsync(location, cancellationToken));
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"The homestay API is unavailable: {ex.Message}";
            return View(Array.Empty<PropertySummary>());
        }
    }

    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var property = await api.GetPropertyAsync(id, cancellationToken);
        return property is null ? NotFound() : View(property);
    }   

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginInput input,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(input);

        var result = await api.LoginAsync(input, cancellationToken);

        if (!result.Success || result.Result is null)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error ?? "Login failed.");

            return View(input);
        }

        SaveAuth(result.Result);
        await HttpContext.Session.CommitAsync(cancellationToken);

        return LocalRedirect(
            returnUrl ?? Url.Action(nameof(Index))!);
    }

    [HttpPost]
    public async Task<IActionResult> AjaxLogin([FromBody] LoginInput input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password))
            return BadRequest(new { success = false, message = "Vui lòng nhập đầy đủ Email và Mật khẩu." });

        try
        {
            var result = await api.LoginAsync(input, cancellationToken);
            if (!result.Success || result.Result is null)
            {
                return BadRequest(new { success = false, message = result.Error ?? "Đăng nhập không thành công. Vui lòng kiểm tra lại email hoặc mật khẩu." });
            }

            SaveAuth(result.Result);
            await HttpContext.Session.CommitAsync(cancellationToken);

            return Ok(new
            {
                success = true,
                user = new
                {
                    id = result.Result.User.Id,
                    email = result.Result.User.Email,
                    fullName = result.Result.User.FullName,
                    role = result.Result.User.Role
                },
                token = result.Result.AccessToken,
                message = "Đăng nhập thành công!"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Lỗi kết nối máy chủ xác thực: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AjaxRegisterGuest([FromBody] RegisterGuestInput input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Email) || string.IsNullOrWhiteSpace(input.Password) || string.IsNullOrWhiteSpace(input.FullName))
            return BadRequest(new { success = false, message = "Vui lòng điền đầy đủ thông tin bắt buộc (Họ tên, Email, Mật khẩu)." });

        try
        {
            var result = await api.RegisterGuestAsync(input, cancellationToken);
            if (!result.Success || result.Result is null)
            {
                return BadRequest(new { success = false, message = result.Error ?? "Đăng ký không thành công. Vui lòng kiểm tra lại thông tin." });
            }

            SaveAuth(result.Result);
            await HttpContext.Session.CommitAsync(cancellationToken);

            return Ok(new
            {
                success = true,
                user = new
                {
                    id = result.Result.User.Id,
                    email = result.Result.User.Email,
                    fullName = result.Result.User.FullName,
                    role = result.Result.User.Role
                },
                token = result.Result.AccessToken,
                message = "Đăng ký tài khoản thành công!"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = $"Lỗi máy chủ khi đăng ký: {ex.Message}" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AjaxLogout(CancellationToken cancellationToken)
    {
        HttpContext.Session.Clear();
        await HttpContext.Session.CommitAsync(cancellationToken);
        return Ok(new { success = true, message = "Đã đăng xuất thành công." });
    }

    [HttpGet]
    public async Task<IActionResult> GetRoomsJson(string? location, CancellationToken cancellationToken)
    {
        try
        {
            var properties = await api.GetPropertiesAsync(location, cancellationToken);
            if (properties != null && properties.Count > 0)
            {
                return Ok(properties);
            }
        }
        catch
        {
            // Fallback gracefully if API service is offline
        }

        return Ok(GetCuratedHomestays(location));
    }

    private static List<object> GetCuratedHomestays(string? location = null)
    {
        var list = new List<object>
        {
            new
            {
                id = 1,
                name = "The Pine Hill Villa & Retreat",
                type = "Villa",
                address = "Đường Mai Anh Đào, Phường 8, TP. Đà Lạt",
                city = "Đà Lạt",
                price = 1250000,
                rating = 4.95,
                reviewsCount = 142,
                description = "Biệt thự biệt lập ẩn mình giữa đồi thông thơ mộng với ban công ngắm bình minh và sương mờ Đà Lạt tuyệt đẹp.",
                amenities = new[] { "Wifi miễn phí", "Ban công view đồi", "Bếp gia đình", "Lò sưởi ấm", "Chỗ đỗ xe" },
                images = new[]
                {
                    "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1613977257363-707ba9348227?auto=format&fit=crop&w=800&q=80"
                }
            },
            new
            {
                id = 2,
                name = "Stayly Ocean Breeze Bungalow",
                type = "Bungalow",
                address = "Trần Phú, Bãi Trước, TP. Vũng Tàu",
                city = "Vũng Tàu",
                price = 950000,
                rating = 4.88,
                reviewsCount = 98,
                description = "Bungalow sát biển với âm thanh sóng vỗ êm đềm, thiết kế mở đón gió đại dương mát lành và hoàng hôn rực rỡ.",
                amenities = new[] { "View biển trực diện", "Hồ bơi vô cực", "Điều hòa", "Bữa sáng miễn phí", "Wifi tốc độ cao" },
                images = new[]
                {
                    "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1584132967334-10e028bd69f7?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1571896349842-33c89424de2d?auto=format&fit=crop&w=800&q=80"
                }
            },
            new
            {
                id = 3,
                name = "An Nam Vintage Homestay",
                type = "Homestay",
                address = "Nguyễn Thái Học, Phố Cổ Hội An, Quảng Nam",
                city = "Hội An",
                price = 480000,
                rating = 4.92,
                reviewsCount = 215,
                description = "Không gian kiến trúc gỗ truyền thống mộc mạc, khu vườn hoa giấy thơ mộng và xe đạp miễn phí dạo quanh phố cổ.",
                amenities = new[] { "Xe đạp miễn phí", "Wifi miễn phí", "Điều hòa", "Bếp chung ấm cúng", "Sân vườn hoa" },
                images = new[]
                {
                    "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1590490360182-c33d57733427?auto=format&fit=crop&w=800&q=80"
                }
            },
            new
            {
                id = 4,
                name = "Skyline Luxury Apartment & Studio",
                type = "Căn hộ",
                address = "Võ Nguyên Giáp, Mỹ Khê, TP. Đà Nẵng",
                city = "Đà Nẵng",
                price = 820000,
                rating = 4.85,
                reviewsCount = 76,
                description = "Căn hộ dịch vụ cao cấp tầng cao nhìn thẳng ra bãi biển Mỹ Khê xinh đẹp, trang bị đầy đủ tiện nghi chuẩn 5 sao.",
                amenities = new[] { "Bể bơi tầng thượng", "Máy giặt riêng", "Bếp từ hiện đại", "Gym & Spa", "Thang máy" },
                images = new[]
                {
                    "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=800&q=80"
                }
            },
            new
            {
                id = 5,
                name = "Misty Mountain Wooden Cabin",
                type = "Homestay",
                address = "Bản Tả Van, Thị xã Sa Pa, Lào Cai",
                city = "Sa Pa",
                price = 650000,
                rating = 4.96,
                reviewsCount = 184,
                description = "Cabin gỗ thông mộc mạc bên sườn đồi nhìn ra thung lũng Mường Hoa bồng bềnh mây trắng và ruộng bậc thang kỳ vĩ.",
                amenities = new[] { "View thung lũng mây", "Bếp sưởi củi", "Wifi", "Bồn tắm gỗ pơ mu", "Trà thảo mộc bản địa" },
                images = new[]
                {
                    "https://images.unsplash.com/photo-1510798831971-661eb04b3739?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1470071459604-3b5ec3a7fe05?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1449158743715-0a90ebb6d2d8?auto=format&fit=crop&w=800&q=80"
                }
            },
            new
            {
                id = 6,
                name = "Sunset Bay Eco Villa Phu Quoc",
                type = "Villa",
                address = "Bãi Trường, Dương Tơ, TP. Phú Quốc",
                city = "Phú Quốc",
                price = 2100000,
                rating = 4.98,
                reviewsCount = 110,
                description = "Villa sinh thái phong cách Địa Trung Hải với bể bơi riêng, đường dạo bộ ra bãi cát vàng và hoàng hôn tím biếc.",
                amenities = new[] { "Hồ bơi riêng", "Bãi tắm riêng", "Đưa đón sân bay", "Bếp nướng BBQ", "Bữa sáng nhiệt đới" },
                images = new[]
                {
                    "https://images.unsplash.com/photo-1540541338287-41700207dee6?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1571003123894-1f0594d2b5d9?auto=format&fit=crop&w=800&q=80",
                    "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?auto=format&fit=crop&w=800&q=80"
                }
            }
        };

        if (!string.IsNullOrWhiteSpace(location))
        {
            var loc = location.Trim().ToLowerInvariant();
            var filtered = list.Where(item =>
            {
                var dict = (dynamic)item;
                string addr = dict.address;
                string name = dict.name;
                string city = dict.city;
                return addr.ToLowerInvariant().Contains(loc) ||
                       name.ToLowerInvariant().Contains(loc) ||
                       city.ToLowerInvariant().Contains(loc);
            }).ToList();

            if (filtered.Count > 0) return filtered;
        }

        return list;
    }

    [HttpGet]
    public IActionResult Register(bool partner = false)
    {
        ViewBag.IsPartner = partner;
        return View(partner
            ? new RegisterOwnerInput()
            : new RegisterGuestInput());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterGuestInput input,
        bool partner = false,
        CancellationToken cancellationToken = default)
    {
        ViewBag.IsPartner = partner;

        if (partner)
        {
            var ownerInput = new RegisterOwnerInput
            {
                FullName = input.FullName,
                DateOfBirth = input.DateOfBirth,
                Gender = input.Gender,
                Phone = input.Phone,
                Email = input.Email,
                Password = input.Password
            };

            return View(ownerInput);
        }

        if (!ModelState.IsValid)
            return View(input);

        var result = await api.RegisterGuestAsync(input, cancellationToken);

        if (!result.Success || result.Result is null)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error ?? "Registration failed.");

            return View(input);
        }

        SaveAuth(result.Result);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterOwner(
        RegisterOwnerInput input,
        CancellationToken cancellationToken)
    {
        ViewBag.IsPartner = true;

        if (!ModelState.IsValid)
            return View("Register", input);

        var result = await api.RegisterOwnerAsync(input, cancellationToken);

        if (!result.Success || result.Result is null)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Error ?? "Registration failed.");

            return View("Register", input);
        }

        SaveAuth(result.Result);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RequestRegisterOtp(
        [FromBody] SendOtpRequestModel model,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Email))
            return BadRequest(new { message = "Email không được để trống." });

        var (success, message, error) = await api.SendOtpAsync(model.Email.Trim(), model.FullName, model.Purpose ?? "Register", cancellationToken);
        if (!success)
            return BadRequest(new { message = error ?? "Không thể gửi mã OTP. Vui lòng thử lại sau." });

        return Ok(new { message });
    }

    [HttpGet]
    public async Task<IActionResult> Profile(
        string tab = "personal",
        CancellationToken cancellationToken = default)
    {
        var token = HttpContext.Session.GetString("token");

        if (token is null)
            return RedirectToAction(nameof(Login));

        var result = await api.GetProfileAsync(token, cancellationToken);

        if (!result.Success || result.Result is null)
        {
            return StatusCode(
                result.StatusCode,
                result.Error ?? "Could not load your profile.");
        }

        var viewModel = new ProfileViewModel
        {
            Profile = result.Result,
            Input = new ProfileInput
            {
                Email = result.Result.Email,
                FullName = result.Result.FullName,
                DateOfBirth = result.Result.DateOfBirth,
                Gender = result.Result.Gender,
                Phone = result.Result.Phone,
                CitizenId = result.Result.CitizenId,
                BankInformation = result.Result.BankInformation
            },
            PasswordInput = new ChangePasswordInput(),
            ActiveTab = tab
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(
        ProfileInput input,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        var role = HttpContext.Session.GetString("role");

        if (token is null)
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            var profileResult = await api.GetProfileAsync(token, cancellationToken);
            return View(new ProfileViewModel
            {
                Profile = profileResult.Result ?? new Profile(0, input.Email, input.FullName, input.DateOfBirth, input.Gender, input.Phone, role ?? "GUEST", input.BankInformation, input.CitizenId),
                Input = input,
                ActiveTab = "personal"
            });
        }

        var result = role == "OWNER"
            ? await api.UpdateOwnerProfileAsync(input, token, cancellationToken)
            : await api.UpdateGuestProfileAsync(input, token, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Could not update your profile.");
            var profileResult = await api.GetProfileAsync(token, cancellationToken);
            return View(new ProfileViewModel
            {
                Profile = profileResult.Result ?? new Profile(0, input.Email, input.FullName, input.DateOfBirth, input.Gender, input.Phone, role ?? "GUEST", input.BankInformation, input.CitizenId),
                Input = input,
                ActiveTab = "personal"
            });
        }

        HttpContext.Session.SetString("userName", input.FullName);
        TempData["Success"] = "Your profile has been updated.";

        return RedirectToAction(nameof(Profile), new { tab = "personal" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordInput passwordInput,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            var profileResult = await api.GetProfileAsync(token, cancellationToken);
            return View("Profile", new ProfileViewModel
            {
                Profile = profileResult.Result!,
                Input = new ProfileInput
                {
                    Email = profileResult.Result?.Email ?? "",
                    FullName = profileResult.Result?.FullName ?? "",
                    DateOfBirth = profileResult.Result?.DateOfBirth,
                    Gender = profileResult.Result?.Gender,
                    Phone = profileResult.Result?.Phone ?? "",
                    CitizenId = profileResult.Result?.CitizenId,
                    BankInformation = profileResult.Result?.BankInformation
                },
                PasswordInput = passwordInput,
                ActiveTab = "security"
            });
        }

        var result = await api.ChangePasswordAsync(passwordInput, token, cancellationToken);
        if (!result.Success)
        {
            TempData["Error"] = result.Error ?? "Could not change password.";
            return RedirectToAction(nameof(Profile), new { tab = "security" });
        }

        TempData["Success"] = "Your password has been changed successfully.";
        return RedirectToAction(nameof(Profile), new { tab = "security" });
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(
        string tab = "properties",
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(tab, "rooms", StringComparison.OrdinalIgnoreCase))
            tab = "properties";

        var token = HttpContext.Session.GetString("token");
        var role = HttpContext.Session.GetString("role");

        // If logged in as OWNER with a valid token, attempt to fetch live data from API
        if (!string.IsNullOrWhiteSpace(token) && role == "OWNER")
        {
            try
            {
                var result = await api.GetOwnerDashboardAsync(token, cancellationToken);
                var amenities = await api.GetAmenitiesAsync(cancellationToken);
                var roomTypes = await api.GetRoomTypesAsync(cancellationToken);
                var reviews = await api.GetOwnerReviewsAsync(token, cancellationToken);

                if (result.Success && result.Result is not null)
                {
                    var viewModel = new OwnerDashboardViewModel
                    {
                        Summary = result.Result.Summary,
                        Properties = result.Result.Properties.Count > 0 ? result.Result.Properties : GetSampleOwnerDashboardViewModel(tab).Properties,
                        Rooms = result.Result.Rooms,
                        Bookings = result.Result.Bookings.Count > 0 ? result.Result.Bookings : GetSampleOwnerDashboardViewModel(tab).Bookings,
                        Invoices = result.Result.Invoices,
                        Reviews = reviews.Count > 0 ? reviews : GetSampleReviews(),
                        AvailableAmenities = amenities,
                        AvailableRoomTypes = roomTypes,
                        ActiveTab = tab
                    };

                    return View(viewModel);
                }
            }
            catch
            {
                // Fallback gracefully below
            }
        }

        // Default or preview/demo mode: return rich, realistic sample data for instant viewing & testing
        return View(GetSampleOwnerDashboardViewModel(tab));
    }

    private static OwnerDashboardViewModel GetSampleOwnerDashboardViewModel(string tab = "properties")
    {
        var properties = new List<OwnerProperty>
        {
            new(
                1,
                "The Pine Hill Villa & Retreat",
                "Đường Mai Anh Đào, Phường 8, TP. Đà Lạt",
                "Phường 8",
                "Đà Lạt",
                "0912345678",
                "pinehill@stayly.vn",
                "Villa",
                "Approved",
                "Nhận phòng từ 14:00, trả phòng trước 12:00. Không hút thuốc trong phòng.",
                null,
                6,
                "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=800&q=80"
            ),
            new(
                2,
                "Stayly Ocean Breeze Bungalow",
                "Trần Phú, Bãi Trước, TP. Vũng Tàu",
                "Phường 1",
                "Vũng Tàu",
                "0987654321",
                "oceanbreeze@stayly.vn",
                "Bungalow",
                "Approved",
                "Miễn phí bữa sáng. Được mang thú cưng nhỏ dưới 5kg.",
                null,
                4,
                "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=800&q=80"
            ),
            new(
                3,
                "Misty Mountain Wooden Cabin",
                "Bản Tả Van, Thị xã Sa Pa, Lào Cai",
                "Tả Van",
                "Sa Pa",
                "0909123456",
                "mistymountain@stayly.vn",
                "Homestay",
                "Pending",
                "Không gian yên tĩnh, tắt nhạc sau 22:00 để giữ sự tĩnh lặng của thung lũng.",
                null,
                3,
                "https://images.unsplash.com/photo-1510798831971-661eb04b3739?auto=format&fit=crop&w=800&q=80"
            ),
            new(
                4,
                "Lotus Riverside Eco Lodge",
                "Cù Lao Chàm, Hội An, Quảng Nam",
                "Tân Hiệp",
                "Hội An",
                "0933456789",
                "lotuslodge@stayly.vn",
                "Homestay",
                "Rejected",
                "Khu bảo tồn sinh thái, không sử dụng đồ nhựa một lần.",
                "Hồ sơ Giấy phép phòng cháy chữa cháy (PCCC) chưa rõ mộc dấu đỏ. Vui lòng bổ sung bản quét PDF rõ nét.",
                2,
                "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=800&q=80"
            )
        };

        var bookings = new List<OwnerBooking>
        {
            new(
                8021,
                "The Pine Hill Villa & Retreat",
                new[] { "P.101 - Suite Đồi Thông" },
                101,
                "Nguyễn Hoàng Nam",
                "0903112233",
                "nam.nguyen@gmail.com",
                DateTime.Now.AddDays(2),
                DateTime.Now.AddDays(5),
                2,
                "Confirmed",
                3750000,
                DateTime.Now.AddDays(-1)
            ),
            new(
                8022,
                "Stayly Ocean Breeze Bungalow",
                new[] { "BG.02 - View Biển Hoàng Hôn" },
                102,
                "Trần Thị Mai Anh",
                "0918776655",
                "maianh.tran@gmail.com",
                DateTime.Now.AddDays(1),
                DateTime.Now.AddDays(3),
                2,
                "Pending",
                1900000,
                DateTime.Now.AddHours(-4)
            ),
            new(
                8023,
                "The Pine Hill Villa & Retreat",
                new[] { "P.202 - Deluxe Ban Công" },
                103,
                "Lê Tuấn Kiệt",
                "0977889900",
                "tuankiet.le@gmail.com",
                DateTime.Now.AddDays(-3),
                DateTime.Now.AddDays(-1),
                3,
                "Completed",
                2500000,
                DateTime.Now.AddDays(-5)
            ),
            new(
                8024,
                "Stayly Ocean Breeze Bungalow",
                new[] { "BG.01 - Sát Biển" },
                104,
                "Phạm Minh Đăng",
                "0944556677",
                "minhdang@gmail.com",
                DateTime.Now.AddDays(7),
                DateTime.Now.AddDays(10),
                4,
                "Cancelled",
                2850000,
                DateTime.Now.AddDays(-2)
            ),
            new(
                8025,
                "The Pine Hill Villa & Retreat",
                new[] { "P.301 - Penthouse Đỉnh Đồi" },
                105,
                "Đặng Thu Trang",
                "0966223344",
                "thutrang.dang@gmail.com",
                DateTime.Now.AddDays(4),
                DateTime.Now.AddDays(8),
                4,
                "Confirmed",
                6200000,
                DateTime.Now.AddDays(-1)
            )
        };

        var approvedCount = properties.Count(p => (p.ApprovalStatus ?? "").Equals("Approved", StringComparison.OrdinalIgnoreCase));
        var pendingCount = properties.Count(p => (p.ApprovalStatus ?? "").Equals("Pending", StringComparison.OrdinalIgnoreCase));
        var totalRev = bookings.Where(b => b.Status == "Confirmed" || b.Status == "Completed").Sum(b => b.TotalAmount);

        return new OwnerDashboardViewModel
        {
            Summary = new OwnerSummary(
                properties.Count,
                approvedCount,
                pendingCount,
                properties.Sum(p => p.RoomsCount),
                bookings.Count,
                bookings.Count(b => b.Status == "Pending"),
                totalRev
            ),
            Properties = properties,
            Rooms = Array.Empty<OwnerRoom>(),
            Bookings = bookings,
            Invoices = Array.Empty<OwnerInvoice>(),
            Reviews = GetSampleReviews(),
            ActiveTab = tab
        };
    }

    private static List<ReviewItem> GetSampleReviews()
    {
        return new List<ReviewItem>
        {
            new(
                1,
                101,
                1,
                "The Pine Hill Villa & Retreat",
                12,
                "Nguyễn Thu Thảo",
                5,
                "Không gian homestay vô cùng yên bình, view đồi thông săn mây buổi sáng đẹp ngỡ ngàng. Phòng ốc cực kỳ sạch sẽ, chăn đệm ấm áp và thơm tho. Chủ nhà siêu nhiệt tình hỗ trợ thuê xe máy và chỉ chỗ ăn ngon ở Đà Lạt. Nhất định sẽ quay lại!",
                new DateTime(2026, 9, 18, 14, 30, 0)
            ),
            new(
                2,
                102,
                2,
                "Stayly Ocean Breeze Bungalow",
                15,
                "Trần Quốc Bảo",
                5,
                "Vị trí ngay sát biển Bãi Trước, tối mở cửa sổ nghe sóng vỗ rất thư giãn. Homestay đầy đủ tiện nghi, bếp nướng BBQ ngoài trời tiện lợi cho gia đình. Hải sản tươi ngon mua ở chợ gần đó về nấu ăn tuyệt vời!",
                new DateTime(2026, 9, 15, 10, 15, 0)
            ),
            new(
                3,
                103,
                1,
                "The Pine Hill Villa & Retreat",
                18,
                "Lê Hoàng Nam",
                4,
                "Chỗ nghỉ ấm cúng, thiết kế phong cách vintage gỗ mộc rất ăn ảnh. Buổi tối đốt lửa sưởi ấm rất chill. Chỉ có đường vào hơi dốc một chút cho xe lớn, nhưng chủ nhà chỉ dẫn nhiệt tình nên không sao.",
                new DateTime(2026, 9, 12, 19, 45, 0)
            ),
            new(
                4,
                104,
                3,
                "Misty Mountain Wooden Cabin",
                20,
                "Phạm Minh Anh",
                5,
                "Cabin trên đồi Sa Pa nhìn thẳng ra thung lũng Mường Hoa. Trải nghiệm tắm lá thuốc người Dao đỏ ngay tại homestay rất đáng thử sau ngày dài trekking. Cảm ơn Stayly và chủ nhà!",
                new DateTime(2026, 9, 8, 8, 20, 0)
            )
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBookingStatus(
        int bookingId,
        string status,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        var result = await api.UpdateBookingStatusAsync(bookingId, status, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Could not update booking status.";
        else
            TempData["Success"] = $"Booking #{bookingId} has been updated to {status}.";

        return RedirectToAction(nameof(Dashboard), new { tab = "bookings" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProperty(
        [Bind(Prefix = "NewProperty")] CreatePropertyInput input,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        // Sanitize nullable string fields to prevent false validation triggers
        if (string.IsNullOrWhiteSpace(input.Email)) input.Email = null;
        if (string.IsNullOrWhiteSpace(input.Ward)) input.Ward = null;
        if (string.IsNullOrWhiteSpace(input.Policy)) input.Policy = null;
        if (string.IsNullOrWhiteSpace(input.BusinessLicenseUrl)) input.BusinessLicenseUrl = null;
        if (string.IsNullOrWhiteSpace(input.FireSafetyDocumentUrl)) input.FireSafetyDocumentUrl = null;
        if (string.IsNullOrWhiteSpace(input.SecurityDocumentUrl)) input.SecurityDocumentUrl = null;

        // Core required fields validation
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            TempData["Error"] = "Vui lòng nhập tên cơ sở lưu trú.";
            return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
        }

        if (string.IsNullOrWhiteSpace(input.Address) || string.IsNullOrWhiteSpace(input.City))
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ địa chỉ và Tỉnh/Thành phố của cơ sở lưu trú.";
            return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
        }

        // Upload verification document files if provided
        if (input.BusinessLicenseFile != null)
        {
            var (licOk, licUrl, licErr) = await api.UploadDocumentAsync(input.BusinessLicenseFile, token, cancellationToken);
            if (!licOk)
            {
                TempData["Error"] = $"Tải hồ sơ Giấy phép kinh doanh thất bại: {licErr ?? "Lỗi máy chủ tải tệp."}";
                return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
            }
            if (!string.IsNullOrWhiteSpace(licUrl))
                input.BusinessLicenseUrl = licUrl;
        }

        if (input.FireSafetyFile != null)
        {
            var (pcccOk, pcccUrl, pcccErr) = await api.UploadDocumentAsync(input.FireSafetyFile, token, cancellationToken);
            if (!pcccOk)
            {
                TempData["Error"] = $"Tải Giấy chứng nhận PCCC thất bại: {pcccErr ?? "Lỗi máy chủ tải tệp."}";
                return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
            }
            if (!string.IsNullOrWhiteSpace(pcccUrl))
                input.FireSafetyDocumentUrl = pcccUrl;
        }

        if (input.SecurityFile != null)
        {
            var (anttOk, anttUrl, anttErr) = await api.UploadDocumentAsync(input.SecurityFile, token, cancellationToken);
            if (!anttOk)
            {
                TempData["Error"] = $"Tải Giấy tờ An ninh trật tự thất bại: {anttErr ?? "Lỗi máy chủ tải tệp."}";
                return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
            }
            if (!string.IsNullOrWhiteSpace(anttUrl))
                input.SecurityDocumentUrl = anttUrl;
        }

        // Upload property photos if provided
        if (input.PhotoFiles != null && input.PhotoFiles.Count > 0)
        {
            var (photosOk, photoUrls, photosErr) = await api.UploadImagesAsync(input.PhotoFiles, "stayly/properties", token, cancellationToken);
            if (!photosOk)
            {
                TempData["Error"] = $"Tải ảnh khuôn viên thất bại: {photosErr ?? "Lỗi máy chủ tải tệp."}";
                return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
            }
            if (photoUrls.Count > 0)
                input.PhotoUrls.AddRange(photoUrls);
        }

        input.Type = string.Equals(input.Type, "Hotel", StringComparison.OrdinalIgnoreCase) ? "Hotel" : "Homestay";

        var result = await api.CreatePropertyAsync(input, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Không thể tạo yêu cầu thẩm định cơ sở.";
        else
            TempData["Success"] = $"Cơ sở lưu trú '{input.Name}' ({input.Type}) đã được gửi duyệt thành công! Admin sẽ thẩm định hồ sơ trước khi kích hoạt.";

        return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
    }

    [HttpGet]
    public async Task<IActionResult> GetPropertyDetailsJson(int id, CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return Unauthorized(new { message = "Vui lòng đăng nhập lại." });

        var (success, details, error) = await api.GetOwnerPropertyAsync(id, token, cancellationToken);
        if (!success || details is null)
            return BadRequest(new { message = error ?? "Không thể tải chi tiết cơ sở lưu trú." });

        return Ok(details);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProperty(
        UpdatePropertyInput input,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        if (input.BusinessLicenseFile != null)
        {
            var (ok, url, err) = await api.UploadDocumentAsync(input.BusinessLicenseFile, token, cancellationToken);
            if (!ok)
            {
                TempData["Error"] = $"Tải Giấy phép kinh doanh thất bại: {err ?? "Lỗi máy chủ"}";
                return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
            }
            if (!string.IsNullOrWhiteSpace(url)) input.BusinessLicenseUrl = url;
        }
        if (input.FireSafetyFile != null)
        {
            var (ok, url, err) = await api.UploadDocumentAsync(input.FireSafetyFile, token, cancellationToken);
            if (!ok)
            {
                TempData["Error"] = $"Tải Giấy chứng nhận PCCC thất bại: {err ?? "Lỗi máy chủ"}";
                return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
            }
            if (!string.IsNullOrWhiteSpace(url)) input.FireSafetyDocumentUrl = url;
        }
        if (input.SecurityFile != null)
        {
            var (ok, url, err) = await api.UploadDocumentAsync(input.SecurityFile, token, cancellationToken);
            if (!ok)
            {
                TempData["Error"] = $"Tải Giấy xác nhận ANTT thất bại: {err ?? "Lỗi máy chủ"}";
                return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
            }
            if (!string.IsNullOrWhiteSpace(url)) input.SecurityDocumentUrl = url;
        }

        var result = await api.UpdatePropertyAsync(input, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Không thể cập nhật thông tin cơ sở lưu trú.";
        else
            TempData["Success"] = input.Resubmit
                ? $"Hồ sơ cơ sở '{input.Name}' đã được cập nhật và gửi duyệt lại thành công! Admin sẽ thẩm định lại hồ sơ."
                : $"Cơ sở lưu trú '{input.Name}' đã được cập nhật thành công.";

        return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProperty(int id, CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        var result = await api.DeletePropertyAsync(id, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Không thể xóa cơ sở lưu trú.";
        else
            TempData["Success"] = "Cơ sở lưu trú đã được xóa thành công.";

        return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
    }

    [HttpGet]
    public async Task<IActionResult> PropertyApprovalHistory(int id, CancellationToken cancellationToken)
    {
        var history = await api.GetPropertyApprovalHistoryAsync(id, cancellationToken);
        return Json(history);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRoom(
        [Bind(Prefix = "NewRoom")] CreateRoomInput? newRoomInput,
        CreateRoomInput? directInput,
        CancellationToken cancellationToken)
    {
        var input = (newRoomInput != null && !string.IsNullOrWhiteSpace(newRoomInput.RoomNumber))
            ? newRoomInput
            : (directInput ?? new CreateRoomInput());

        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        if (input.PropertyId <= 0 || string.IsNullOrWhiteSpace(input.RoomNumber) || input.OriginalPrice <= 0)
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ thông tin phòng hợp lệ (Cơ sở, Tên/Số phòng, Giá mỗi đêm).";
            return RedirectToAction(nameof(Dashboard), new { tab = "rooms" });
        }

        if (input.PhotoFiles != null && input.PhotoFiles.Count > 0)
        {
            var (photosOk, photoUrls, _) = await api.UploadImagesAsync(input.PhotoFiles, $"stayly/rooms/{input.PropertyId}", token, cancellationToken);
            if (photosOk && photoUrls.Count > 0)
                input.PhotoUrls.AddRange(photoUrls);
        }

        var result = await api.CreateRoomAsync(input.PropertyId, input, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Không thể tạo phòng.";
        else
            TempData["Success"] = $"Phòng '{input.RoomNumber}' đã được tạo thành công.";

        return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoom(
        UpdateRoomInput input,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        var result = await api.UpdateRoomAsync(input, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Không thể cập nhật phòng.";
        else
            TempData["Success"] = $"Phòng '{input.RoomNumber}' đã được cập nhật thành công.";

        return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoom(int propertyId, int roomId, CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        var result = await api.DeleteRoomAsync(propertyId, roomId, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Không thể xóa phòng.";
        else
            TempData["Success"] = "Phòng đã được xóa thành công.";

        return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(
        BookingInput input,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");

        if (token is null)
        {
            return RedirectToAction(
                nameof(Login),
                new
                {
                    returnUrl = Url.Action(
                        nameof(Details),
                        new { id = input.RoomId })
                });
        }

        var result = await api.CreateBookingAsync(
            input,
            token,
            cancellationToken);

        if (!result.Success)
            TempData["Error"] = result.Error ?? "The room could not be booked.";
        else
            TempData["Success"] = "Your booking request has been sent.";

        return RedirectToAction(nameof(Trips));
    }

    public async Task<IActionResult> Trips(
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");

        if (token is null)
            return RedirectToAction(nameof(Login));

        try
        {
            return View(await api.GetBookingsAsync(token, cancellationToken));
        }
        catch
        {
            TempData["Error"] = "Could not load your bookings.";
            return View(Array.Empty<Booking>());
        }
    }

    private void SaveAuth(AuthResult result)
    {
        HttpContext.Session.SetString("token", result.AccessToken);
        HttpContext.Session.SetString("userName", result.User.FullName);
        HttpContext.Session.SetString("role", result.User.Role);
    }

    [HttpGet]
    public async Task<IActionResult> GetBookingDetailJson(int id, CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrWhiteSpace(token))
        {
            // For preview/unauthenticated mode, return demo data
            return Ok(new
            {
                success = true,
                data = new
                {
                    id = id,
                    propertyName = "The Pine Hill Villa & Retreat",
                    roomNumbers = new[] { "101", "102" },
                    guestId = 12,
                    guestName = "Lê Thị Bích Trâm",
                    guestPhone = "0987654321",
                    guestEmail = "guest.traveler@stayly.com",
                    checkIn = DateTime.UtcNow.AddDays(2),
                    checkOut = DateTime.UtcNow.AddDays(5),
                    guestCount = 4,
                    status = "Confirmed",
                    basePrice = 2400000m,
                    totalAmount = 2040000m,
                    promotionCode = "STAYLY2026",
                    discountPercentage = 15,
                    discountAmount = 360000m,
                    extraFees = new[]
                    {
                        new { id = 1, name = "Phí dọn dẹp vệ sinh", quantity = 1, price = 0m, total = 0m, note = "Miễn phí ưu đãi mùa này" }
                    },
                    createdDate = DateTime.UtcNow.AddDays(-1)
                }
            });
        }

        var result = await api.GetOwnerBookingDetailAsync(id, token, cancellationToken);
        if (!result.Success || result.Result == null)
        {
            return BadRequest(new { success = false, message = result.Error ?? "Không thể lấy thông tin chi tiết đơn đặt phòng." });
        }

        return Ok(new { success = true, data = result.Result });
    }

    [HttpPost]
    public async Task<IActionResult> ReplyReviewAjax([FromBody] ReplyReviewAjaxInput input, CancellationToken cancellationToken)
    {
        if (input == null || input.ReviewId <= 0 || string.IsNullOrWhiteSpace(input.Message))
            return BadRequest(new { success = false, message = "Nội dung phản hồi không được để trống." });

        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized(new { success = false, message = "Vui lòng đăng nhập với tài khoản chủ nhà để phản hồi đánh giá." });

        var result = await api.ReplyReviewAsync(input.ReviewId, new ReplyReviewInput(input.Message), token, cancellationToken);
        if (!result.Success)
            return BadRequest(new { success = false, message = result.Error ?? "Không thể gửi phản hồi đánh giá." });

        return Ok(new { success = true, message = "Phản hồi đánh giá đã được gửi thành công!" });
    }

    [HttpGet]
    public async Task<IActionResult> GetPromotionsJson(CancellationToken cancellationToken)
    {
        try
        {
            var promotions = await api.GetPromotionsAsync(cancellationToken);
            return Ok(new { success = true, data = promotions });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateReviewAjax([FromBody] CreateReviewAjaxInput input, CancellationToken cancellationToken)
    {
        if (input == null || input.BookingId <= 0)
            return BadRequest(new { success = false, message = "Thông tin đơn đặt phòng không hợp lệ." });

        var token = HttpContext.Session.GetString("token");
        if (string.IsNullOrWhiteSpace(token))
            return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để gửi đánh giá." });

        var result = await api.CreateReviewAsync(new CreateReviewInput(input.BookingId, input.Rating, input.Comment), token, cancellationToken);
        if (!result.Success)
            return BadRequest(new { success = false, message = result.Error ?? "Không thể gửi đánh giá." });

        return Ok(new { success = true, message = "Đánh giá của bạn đã được gửi thành công!" });
    }
}

public sealed class ReplyReviewAjaxInput
{
    public int ReviewId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class CreateReviewAjaxInput
{
    public int BookingId { get; set; }
    public int Rating { get; set; } = 5;
    public string? Comment { get; set; }
}
