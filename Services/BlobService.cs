using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

/*
==============================Code Attribution==================================
Azure Blob Storage Client Library for .NET
Author: Microsoft
Link: https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-dotnet-get-started
Date Accessed: 29 April 2026
==============================Code Attribution==================================
*/

namespace MediBook.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        // Allowed image file extensions
        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        // Maximum file size: 5 MB
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public BlobService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Blob storage connection string 'AzureBlobStorage' is missing in appsettings.json.");

            _containerName = configuration["BlobContainerName"] ?? "facility-images";
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        // ── Uploads an image file to Azurite and returns its public URL ──
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            // Validate: file must not be null or empty
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was provided for upload.");

            // Validate: file size must not exceed the maximum allowed
            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException(
                    $"File size exceeds the maximum allowed size of 5 MB. " +
                    $"Your file is {(file.Length / 1024.0 / 1024.0):F1} MB.");

            // Validate: file extension must be an allowed image type
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException(
                    $"Invalid file type '{extension}'. " +
                    $"Allowed types are: {string.Join(", ", AllowedExtensions)}.");

            try
            {
                // Get or create the container with public blob access
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                // Generate a unique filename to prevent overwriting existing blobs
                var fileName = $"{Guid.NewGuid()}{extension}";
                var blobClient = containerClient.GetBlobClient(fileName);

                // Upload the file stream to Azurite
                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);

                return blobClient.Uri.ToString();
            }
            catch (ArgumentException)
            {
                // Re-throw our own validation errors as-is
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "An error occurred while uploading the image to blob storage. " +
                    "Please ensure Azurite is running and try again. " +
                    $"Details: {ex.Message}");
            }
        }

        // ── Deletes a blob by its full URL ──
        public async Task DeleteImageAsync(string? imageUrl)
        {
            // Skip deletion if no URL is provided or it is a local placeholder
            if (string.IsNullOrWhiteSpace(imageUrl)) return;
            if (!imageUrl.StartsWith("http://127.0.0.1")) return;

            try
            {
                // Extract the blob filename from the full URL
                var uri = new Uri(imageUrl);
                var blobName = Path.GetFileName(uri.LocalPath);

                if (string.IsNullOrWhiteSpace(blobName)) return;

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                // Silently handle deletion failures —
                // the blob may have already been deleted or never existed.
                // We do not want a failed image deletion to crash the app.
            }
        }

        // ── Checks whether Azurite is reachable ──
        // Useful for a health check or startup diagnostic
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                await foreach (var _ in _blobServiceClient.GetBlobContainersAsync())
                    break; // Just need one successful response
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}