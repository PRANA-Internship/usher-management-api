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
        Task<string> GetPresignedUrlAsync(string objectPath, int expirySeconds = 3600, CancellationToken ct = default);
        Task DeleteAsync(string objectPath, CancellationToken ct = default);
    }

}
