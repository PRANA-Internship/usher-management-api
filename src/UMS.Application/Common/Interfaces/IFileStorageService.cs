using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            string folder,
            CancellationToken ct = default);
        Task DeleteAsync(string objectPath, CancellationToken ct = default);

        Task<Stream> DownloadFileAsync(string objectPath, CancellationToken ct = default);

        Task<bool> FileExistsAsync(string objectPath, CancellationToken ct = default);

        Task<string> MoveFileAsync(string sourcePath, string destinationFolder, CancellationToken ct = default);
    }
}