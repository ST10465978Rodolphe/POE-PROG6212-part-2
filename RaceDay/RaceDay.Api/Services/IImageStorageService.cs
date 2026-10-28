namespace RaceDay.Api.Services;

// Everything the API needs from cloud image storage.
// Keeping it behind an interface means tests can use a fake instead of real Azure.
public interface IImageStorageService
{
    // Returns an error message if the file is not acceptable, or null if it is fine
    string? Validate(IFormFile? file);

    // Uploads the image into the given folder and returns its public URL
    Task<string> UploadAsync(IFormFile file, string folder);

    // Deletes an image that was uploaded earlier (used when an image is replaced)
    Task DeleteAsync(string? url);
}