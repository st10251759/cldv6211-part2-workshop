using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

/*
==============================Code Attribution==================================
Azure Blob Storage Client Library for .NET
Author: Microsoft
Link: [https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-dotnet-get-started](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-dotnet-get-started)
Date Accessed: 29 April 2026
==============================Code Attribution==================================
*/

namespace MediBook.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        private static readonly string[] AllowedExtensions =
            { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public BlobService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureBlobStorage");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Blob storage connection string 'AzureBlobStorage' is missing in appsettings.json.");
            }

            _containerName = configuration["BlobContainerName"] ?? "facility-images";
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        // Uploads an image file to Azure Blob Storage and returns its public URL.
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was provided for upload.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException(
                    $"File size exceeds the maximum allowed size of 5 MB. Your file is {(file.Length / 1024.0 / 1024.0):F1} MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new ArgumentException(
                    $"Invalid file type '{extension}'. Allowed types are: {string.Join(", ", AllowedExtensions)}.");
            }

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var blobClient = containerClient.GetBlobClient(fileName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);

                return blobClient.Uri.ToString();
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "An error occurred while uploading the image to Azure Blob Storage. " +
                    $"Details: {ex.Message}");
            }
        }

        // Deletes a blob by its full URL.
        public async Task DeleteImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;
            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)) return;

            try
            {
                var blobName = Path.GetFileName(uri.LocalPath);
                if (string.IsNullOrWhiteSpace(blobName)) return;

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();
            }
            catch
            {
                // Ignore deletion errors so a missing blob does not break the app.
            }
        }

        // Checks whether Azure Blob Storage is reachable.
        public async Task<bool> IsAvailableAsync()
        {
            try
            {
                await foreach (var _ in _blobServiceClient.GetBlobContainersAsync())
                    break;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}