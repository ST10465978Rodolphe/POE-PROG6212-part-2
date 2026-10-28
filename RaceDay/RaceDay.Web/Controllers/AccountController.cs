using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Web.Models;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers;

// Registration, login and logout. All the real work is done by the API:
// this controller only collects the form, calls the API and keeps the session.
public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api)
    {
        _api = api;
    }

    // ---------- Register ----------

    [HttpGet]
    public IActionResult Register()
    {
        if (HttpContext.Session.IsLoggedIn())
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _api.PostAsync<AuthResponse>("api/auth/register", new RegisterRequest
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim(),
            Password = model.Password,
            Role = model.Role,
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim()
        });

        if (!result.Success)
        {
            // For example: "An account with this email already exists."
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        TempData["Success"] = "Your account was created. Please log in.";
        return RedirectToAction(nameof(Login));
    }

    // ---------- Login ----------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (HttpContext.Session.IsLoggedIn())
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _api.LoginAsync(new LoginRequest
        {
            Email = model.Email.Trim(),
            Password = model.Password
        });

        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Error);
            return View(model);
        }

        // Remember who is logged in and their role for all the following pages
        HttpContext.Session.SignIn(result.Data);

        // Only follow a return address that belongs to this site
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    // ---------- Logout ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // End the session in the API, then forget everything here
        await _api.PostAsync<MessageResponse>("api/auth/logout", null);
        HttpContext.Session.Clear();

        return RedirectToAction(nameof(Login));
    }

    // ---------- Access denied ----------

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}