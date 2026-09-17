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
        var token = HttpContext.Session.GetString("token");
        var role = HttpContext.Session.GetString("role");

        if (token is null)
            return RedirectToAction(nameof(Login), new { returnUrl = Url.Action(nameof(Dashboard)) });

        if (role != "OWNER")
        {
            TempData["Error"] = "Owner Dashboard is only accessible by property owners.";
            return RedirectToAction(nameof(Index));
        }

        var result = await api.GetOwnerDashboardAsync(token, cancellationToken);
        var amenities = await api.GetAmenitiesAsync(cancellationToken);
        var roomTypes = await api.GetRoomTypesAsync(cancellationToken);

        if (!result.Success || result.Result is null)
        {
            TempData["Error"] = result.Error ?? "Could not load owner dashboard.";
            return View(new OwnerDashboardViewModel
            {
                ActiveTab = tab,
                AvailableAmenities = amenities,
                AvailableRoomTypes = roomTypes
            });
        }

        var viewModel = new OwnerDashboardViewModel
        {
            Summary = result.Result.Summary,
            Properties = result.Result.Properties,
            Rooms = result.Result.Rooms,
            Bookings = result.Result.Bookings,
            Invoices = result.Result.Invoices,
            AvailableAmenities = amenities,
            AvailableRoomTypes = roomTypes,
            ActiveTab = tab
        };

        return View(viewModel);
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
        [Bind(Prefix = "NewProperty")] CreatePropertyInput? newPropertyInput,
        CreatePropertyInput? directInput,
        CancellationToken cancellationToken)
    {
        var input = (newPropertyInput != null && !string.IsNullOrWhiteSpace(newPropertyInput.Name))
            ? newPropertyInput
            : (directInput ?? new CreatePropertyInput());

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
            var (licOk, licUrl, _) = await api.UploadDocumentAsync(input.BusinessLicenseFile, token, cancellationToken);
            if (licOk && !string.IsNullOrWhiteSpace(licUrl))
                input.BusinessLicenseUrl = licUrl;
        }

        if (input.FireSafetyFile != null)
        {
            var (pcccOk, pcccUrl, _) = await api.UploadDocumentAsync(input.FireSafetyFile, token, cancellationToken);
            if (pcccOk && !string.IsNullOrWhiteSpace(pcccUrl))
                input.FireSafetyDocumentUrl = pcccUrl;
        }

        if (input.SecurityFile != null)
        {
            var (anttOk, anttUrl, _) = await api.UploadDocumentAsync(input.SecurityFile, token, cancellationToken);
            if (anttOk && !string.IsNullOrWhiteSpace(anttUrl))
                input.SecurityDocumentUrl = anttUrl;
        }

        // Upload property photos if provided
        if (input.PhotoFiles != null && input.PhotoFiles.Count > 0)
        {
            var (photosOk, photoUrls, _) = await api.UploadImagesAsync(input.PhotoFiles, "stayly/properties", token, cancellationToken);
            if (photosOk && photoUrls.Count > 0)
                input.PhotoUrls.AddRange(photoUrls);
        }

        var result = await api.CreatePropertyAsync(input, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Không thể tạo yêu cầu thẩm định cơ sở.";
        else
            TempData["Success"] = $"Homestay '{input.Name}' đã được gửi duyệt thành công! Admin sẽ thẩm định hồ sơ trước khi hiển thị công khai trên website.";

        return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
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
            var (ok, url, _) = await api.UploadDocumentAsync(input.BusinessLicenseFile, token, cancellationToken);
            if (ok && !string.IsNullOrWhiteSpace(url)) input.BusinessLicenseUrl = url;
        }
        if (input.FireSafetyFile != null)
        {
            var (ok, url, _) = await api.UploadDocumentAsync(input.FireSafetyFile, token, cancellationToken);
            if (ok && !string.IsNullOrWhiteSpace(url)) input.FireSafetyDocumentUrl = url;
        }
        if (input.SecurityFile != null)
        {
            var (ok, url, _) = await api.UploadDocumentAsync(input.SecurityFile, token, cancellationToken);
            if (ok && !string.IsNullOrWhiteSpace(url)) input.SecurityDocumentUrl = url;
        }

        var result = await api.UpdatePropertyAsync(input, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Không thể cập nhật thông tin homestay.";
        else
            TempData["Success"] = "Cơ sở lưu trú đã được cập nhật thành công.";

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

        return RedirectToAction(nameof(Dashboard), new { tab = "rooms" });
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

        return RedirectToAction(nameof(Dashboard), new { tab = "rooms" });
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

        return RedirectToAction(nameof(Dashboard), new { tab = "rooms" });
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
}
