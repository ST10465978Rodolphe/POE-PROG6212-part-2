using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

// Stores images in an Azure Blob Storage container.
// The connection string comes from configuration (user secrets locally,
// an environment variable in Docker), never from the source code.
public class AzureBlobImageService : IImageStorageService
{
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly Dictionary<string, string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

    private readonly BlobContainerClient _container;

    public AzureBlobImageService(IConfiguration configuration)
    {
        string? connectionString = configuration["AzureStorage:ConnectionString"];
        string? containerName = configuration["AzureStorage:ContainerName"];

        if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(containerName))
        {
            throw new InvalidOperationException(
                "Azure Storage is not configured. Set AzureStorage:ConnectionString and " +
                "AzureStorage:ContainerName (user secrets or environment variables).");
        }

        _container = new BlobContainerClient(connectionString, containerName);
    }

    public string? Validate(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return "Please choose an image file.";
        }

        if (file.Length > MaxBytes)
        {
            return "The image must be 5 MB or smaller.";
        }

        string extension = Path.GetExtension(file.FileName);
        if (!AllowedTypes.ContainsKey(extension))
        {
            return "Only .jpg, .jpeg, .png and .webp images are allowed.";
        }

        return null;
    }

    public async Task<string> UploadAsync(IFormFile file, string folder)
    {
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        // A random name avoids clashes and never trusts the name the user chose
        string blobName = $"{folder}/{Guid.NewGuid():N}{extension}";
        var blob = _container.GetBlobClient(blobName);

        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = AllowedTypes[extension] }
        });

        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            // Only delete blobs that live in our own container
            string prefix = "/" + _container.Name + "/";
            string path = Uri.UnescapeDataString(new Uri(url).AbsolutePath);

            if (path.StartsWith(prefix, StringComparison.Ordinal))
            {
                await _container.DeleteBlobIfExistsAsync(path.Substring(prefix.Length));
            }
        }
        catch (Exception)
        {
            // Removing the old image is a clean-up step. If it fails, the new image
            // is already saved, so the user should not see an error.
        }
    }
}