using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Application.Interfaces
{
    public interface IStorageService
    {
        // Return SeaweedFS id, after upload to storage
        Task<string> UploadAsync(Stream content, string contentType, CancellationToken ct = default);
        Task<Stream> Download(string storageKey, CancellationToken ct = default);
        Task DeleteAsync(string storageKey, CancellationToken ct = default); 
        Task<string> GetDownloadUrlAsync(string storageKey, CancellationToken ct = default)
    }
}
