using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Services;
using RaceDay.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
[RequireRole] // must be logged in; the role is set per action
public class UploadsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IImageStorageService _images;

    public UploadsController(RaceDayDbContext db, IImageStorageService images)
    {
        _db = db;
        _images = images;
    }

    [HttpPost("events/{eventId:int}/banner")]
    [RequireRole("Organiser")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Upload an event banner",
        Description = "Organiser only, and only for events they own. Send the image as multipart form data in a field called 'file' (.jpg, .jpeg, .png or .webp, maximum 5 MB). The image is stored in Azure Blob Storage and its URL is saved on the event. Any previous banner is removed.")]
    [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> UploadBanner(int eventId, IFormFile file)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (ev.OrganiserId != HttpContext.GetUserId())
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "You can only upload a banner for your own events." });
        }

        string? error = _images.Validate(file);
        if (error != null)
        {
            return BadRequest(new { message = error });
        }

        string? url = await TryUpload(file, $"events/{eventId}");
        if (url == null)
        {
            return StorageUnavailable();
        }

        string? oldUrl = ev.BannerUrl;
        ev.BannerUrl = url;
        await _db.SaveChangesAsync();
        await _images.DeleteAsync(oldUrl);

        return Ok(new UploadResponse { Url = url });
    }

    [HttpPost("users/me/profile-picture")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Upload my profile picture",
        Description = "Both roles. Send the image as multipart form data in a field called 'file' (.jpg, .jpeg, .png or .webp, maximum 5 MB). The image is stored in Azure Blob Storage and its URL is saved on the logged-in user's profile. Any previous picture is removed.")]
    [ProducesResponseType(typeof(UploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        int userId = HttpContext.GetUserId();

        var user = await _db.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        string? error = _images.Validate(file);
        if (error != null)
        {
            return BadRequest(new { message = error });
        }

        string? url = await TryUpload(file, $"profiles/{userId}");
        if (url == null)
        {
            return StorageUnavailable();
        }

        string? oldUrl = user.ProfilePictureUrl;
        user.ProfilePictureUrl = url;
        await _db.SaveChangesAsync();
        await _images.DeleteAsync(oldUrl);

        return Ok(new UploadResponse { Url = url });
    }

    // Returns the URL of the saved image, or null if Azure could not be reached
    private async Task<string?> TryUpload(IFormFile file, string folder)
    {
        try
        {
            return await _images.UploadAsync(file, folder);
        }
        catch (Azure.RequestFailedException)
        {
            return null;
        }
    }

    private ObjectResult StorageUnavailable() =>
        StatusCode(StatusCodes.Status502BadGateway,
            new { message = "The image could not be saved to cloud storage. Please try again." });
}