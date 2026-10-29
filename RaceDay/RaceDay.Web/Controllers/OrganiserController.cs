using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;

namespace RaceDay.Web.Controllers;

// Every page in this controller is for Organisers only.
// The pages are placeholders for now: they are built in the next steps.
[RequireLogin("Organiser")]
public class OrganiserController : Controller
{
    public IActionResult Dashboard() => View("ComingSoon", "Dashboard");

    public IActionResult Events() => View("ComingSoon", "My Events");
}