using GodotXR.Application.Services;
using GodotXR.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace GodotXR.Infrastructure.Core
{
    public sealed class MinIOService : IStorageService
    {
        private readonly IMinioClient _client;
        private readonly StorageOptions _options;

        public MinIOService(
            IMinioClient client,
            IOptions<StorageOptions> options)
        {
            _client = client;
            _options = options.Value;
        }

        public async Task<string> UploadAsync(
            Stream stream,
            string fileName,
            string contentType,
            CancellationToken ct)
        {
            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs()
                    .WithBucket(_options.BucketName),
                ct);

            if (!exists)
            {
                await _client.MakeBucketAsync(
                    new MakeBucketArgs()
                        .WithBucket(_options.BucketName),
                    ct);
            }

            await _client.PutObjectAsync(
                new PutObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType),
                ct);

            return fileName;
        }

        public async Task<string> GetPresignedUrlAsync(
            string objectName,
            int expirySeconds,
            CancellationToken ct)
        {
            return await _client.PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(objectName)
                    .WithExpiry(expirySeconds));
        }

        public async Task DeleteAsync(
            string objectName,
            CancellationToken ct)
        {
            await _client.RemoveObjectAsync(
                new RemoveObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(objectName),
                ct);
        }
    }
}
