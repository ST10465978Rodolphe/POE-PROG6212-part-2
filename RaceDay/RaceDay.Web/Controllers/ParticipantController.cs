using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;

namespace RaceDay.Web.Controllers;

// Every page in this controller is for Participants only.
// The pages are placeholders for now: they are built in the next steps.
[RequireLogin("Participant")]
public class ParticipantController : Controller
{
    public IActionResult Events() => View("ComingSoon", "Browse Events");

    public IActionResult MyEvents() => View("ComingSoon", "My Events");

    public IActionResult MyResults() => View("ComingSoon", "My Results");
}