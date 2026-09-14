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

    [HttpGet]
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
        if (!result.Success || result.Result is null)
        {
            TempData["Error"] = result.Error ?? "Could not load owner dashboard.";
            return View(new OwnerDashboardViewModel { ActiveTab = tab });
        }

        var viewModel = new OwnerDashboardViewModel
        {
            Summary = result.Result.Summary,
            Properties = result.Result.Properties,
            Rooms = result.Result.Rooms,
            Bookings = result.Result.Bookings,
            Invoices = result.Result.Invoices,
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
        CreatePropertyInput input,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please provide all required property details.";
            return RedirectToAction(nameof(Dashboard), new { tab = "properties" });
        }

        var result = await api.CreatePropertyAsync(input, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Could not create property.";
        else
            TempData["Success"] = $"Homestay '{input.Name}' has been submitted for Admin verification via WPF. It will appear on the public website and be ready for room setup once approved by Admin.";

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
        CreateRoomInput input,
        CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("token");
        if (token is null)
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please fill in all room fields.";
            return RedirectToAction(nameof(Dashboard), new { tab = "rooms" });
        }

        var result = await api.CreateRoomAsync(input.PropertyId, input, token, cancellationToken);
        if (!result.Success)
            TempData["Error"] = result.Error ?? "Could not add room.";
        else
            TempData["Success"] = $"Room '{input.RoomNumber}' has been added successfully.";

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
