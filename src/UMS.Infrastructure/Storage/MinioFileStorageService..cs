using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using System;
using System.Collections.Generic;
using System.Text;
using UMS.Application.Common.Interfaces;
using UMS.Infrastructure.Settings;

namespace UMS.Infrastructure.Storage
{
    public sealed class MinioFileStorageService(
     IMinioClient minioClient,
     IOptions<MinioSettings> options
 ) : IFileStorageService
    {
        private readonly MinioSettings _settings = options.Value;

        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken ct = default)
        {
            await EnsureBucketExistsAsync(ct);

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var objectName = $"{folder}/{Guid.NewGuid()}{extension}";

            var putArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            await minioClient.PutObjectAsync(putArgs, ct);

            return objectName;
        }

        public async Task DeleteAsync(string objectPath, CancellationToken ct = default)
        {
            var removeArgs = new RemoveObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectPath);

            await minioClient.RemoveObjectAsync(removeArgs, ct);
        }

        private async Task EnsureBucketExistsAsync(CancellationToken ct)
        {
            var existsArgs = new BucketExistsArgs().WithBucket(_settings.BucketName);
            var exists = await minioClient.BucketExistsAsync(existsArgs, ct);

            if (!exists)
            {
                var makeArgs = new MakeBucketArgs().WithBucket(_settings.BucketName);
                await minioClient.MakeBucketAsync(makeArgs, ct);
            }
        }
    }
}
