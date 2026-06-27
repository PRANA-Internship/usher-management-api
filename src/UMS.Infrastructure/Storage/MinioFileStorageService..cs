using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Options;

using Minio;
using Minio.DataModel.Args;

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

        public async Task<Stream> DownloadFileAsync(string objectPath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(objectPath))
                return Stream.Null;

            var memoryStream = new MemoryStream();

            var args = new GetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectPath)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            await minioClient.GetObjectAsync(args, ct);
            memoryStream.Position = 0;
            return memoryStream;
        }

        public async Task<bool> FileExistsAsync(string objectPath, CancellationToken ct = default)
        {
            try
            {
                var args = new StatObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(objectPath);

                await minioClient.StatObjectAsync(args, ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> MoveFileAsync(
            string sourcePath,
            string destinationFolder,
            CancellationToken ct = default)
        {
            var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            var destinationPath = $"{destinationFolder}/{Guid.NewGuid()}{extension}";

            var copyArgs = new CopyObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(destinationPath)
                .WithCopyObjectSource(new CopySourceObjectArgs()
                    .WithBucket(_settings.BucketName)
                    .WithObject(sourcePath));

            await minioClient.CopyObjectAsync(copyArgs, ct);
            await DeleteAsync(sourcePath, ct);
            return destinationPath;
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