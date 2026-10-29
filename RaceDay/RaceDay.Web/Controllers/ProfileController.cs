using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;

namespace RaceDay.Web.Controllers;

// Both roles can view and update their own profile.
// The page is a placeholder for now: it is built in a later step.
[RequireLogin]
public class ProfileController : Controller
{
    public IActionResult Index() => View("ComingSoon", "My Profile");
}