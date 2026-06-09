namespace GodotXR.Application.Services
{
    public interface IStorageService
    {
        Task<string> UploadAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken ct);

        Task<string> GetPresignedUrlAsync(
            string objectName,
            int expirySeconds,
            CancellationToken ct);

        Task DeleteAsync(
            string objectName,
            CancellationToken ct);
    }
}
