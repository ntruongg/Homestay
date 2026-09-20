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
        try
        {
            var property = await api.GetPropertyAsync(id, cancellationToken);
            if (property is null)
            {
                TempData["Error"] = $"Không tìm thấy cơ sở lưu trú #{id}.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                ViewBag.Reviews = await api.GetPropertyReviewsAsync(id, cancellationToken);
            }
            catch
            {
                ViewBag.Reviews = Array.Empty<ReviewItem>();
            }

            try
            {
                ViewBag.Amenities = await api.GetAmenitiesAsync(cancellationToken);
            }
            catch
            {
                ViewBag.Amenities = Array.Empty<AmenityItem>();
            }

            return View(property);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Lỗi khi tải thông tin: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
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
            return Ok(properties ?? Array.Empty<PropertySummary>());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
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
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(nameof(Login), new { returnUrl = Url.Action(nameof(Dashboard), new { tab }) });
        }

        try
        {
            var result = await api.GetOwnerDashboardAsync(token, cancellationToken);
            var amenities = await api.GetAmenitiesAsync(cancellationToken);
            var roomTypes = await api.GetRoomTypesAsync(cancellationToken);
            var reviews = await api.GetOwnerReviewsAsync(token, cancellationToken);

            var summary = result.Result?.Summary ?? new OwnerSummary(0, 0, 0, 0, 0, 0, 0);
            var properties = result.Result?.Properties ?? (IReadOnlyList<OwnerProperty>)Array.Empty<OwnerProperty>();
            var rooms = result.Result?.Rooms ?? (IReadOnlyList<OwnerRoom>)Array.Empty<OwnerRoom>();
            var bookings = result.Result?.Bookings ?? (IReadOnlyList<OwnerBooking>)Array.Empty<OwnerBooking>();
            var invoices = result.Result?.Invoices ?? (IReadOnlyList<OwnerInvoice>)Array.Empty<OwnerInvoice>();

            var viewModel = new OwnerDashboardViewModel
            {
                Summary = summary,
                Properties = properties,
                Rooms = rooms,
                Bookings = bookings,
                Invoices = invoices,
                Reviews = reviews ?? Array.Empty<ReviewItem>(),
                AvailableAmenities = amenities ?? Array.Empty<AmenityItem>(),
                AvailableRoomTypes = roomTypes ?? Array.Empty<RoomTypeItem>(),
                ActiveTab = tab
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Không thể tải bảng điều khiển: {ex.Message}";
            var viewModel = new OwnerDashboardViewModel
            {
                Summary = new OwnerSummary(0, 0, 0, 0, 0, 0, 0),
                Properties = Array.Empty<OwnerProperty>(),
                Rooms = Array.Empty<OwnerRoom>(),
                Bookings = Array.Empty<OwnerBooking>(),
                Invoices = Array.Empty<OwnerInvoice>(),
                Reviews = Array.Empty<ReviewItem>(),
                AvailableAmenities = Array.Empty<AmenityItem>(),
                AvailableRoomTypes = Array.Empty<RoomTypeItem>(),
                ActiveTab = tab
            };
            return View(viewModel);
        }
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
        [Bind(Prefix = "NewProperty")] CreatePropertyInput? prefixedInput,
        CreatePropertyInput? directInput,
        CancellationToken cancellationToken)
    {
        var input = (prefixedInput != null && !string.IsNullOrWhiteSpace(prefixedInput.Name))
            ? prefixedInput
            : (directInput ?? new CreatePropertyInput());

        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        // Trim and sanitize string fields
        input.Name = input.Name?.Trim() ?? string.Empty;
        input.Phone = input.Phone?.Trim();
        input.Address = input.Address?.Trim() ?? string.Empty;
        input.City = input.City?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(input.Email)) input.Email = null; else input.Email = input.Email.Trim();
        if (string.IsNullOrWhiteSpace(input.Ward)) input.Ward = null; else input.Ward = input.Ward.Trim();
        if (string.IsNullOrWhiteSpace(input.Policy)) input.Policy = null; else input.Policy = input.Policy.Trim();
        if (string.IsNullOrWhiteSpace(input.BusinessLicenseUrl)) input.BusinessLicenseUrl = null;
        if (string.IsNullOrWhiteSpace(input.FireSafetyDocumentUrl)) input.FireSafetyDocumentUrl = null;
        if (string.IsNullOrWhiteSpace(input.SecurityDocumentUrl)) input.SecurityDocumentUrl = null;

        // Core required fields validation
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            TempData["Error"] = "Vui lòng nhập tên cơ sở lưu trú.";
            TempData["OpenModal"] = "createPropertyModal";
            return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
        }

        if (string.IsNullOrWhiteSpace(input.Address) || string.IsNullOrWhiteSpace(input.City))
        {
            TempData["Error"] = "Vui lòng nhập đầy đủ địa chỉ và Tỉnh/Thành phố của cơ sở lưu trú.";
            TempData["OpenModal"] = "createPropertyModal";
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
