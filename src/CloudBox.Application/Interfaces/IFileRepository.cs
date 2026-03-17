using CloudBox.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Application.Interfaces
{
    public interface IFileRepository
    {
        Task<FileItem> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<FileItem> GetByPathAsync(Guid onwerId, string path, CancellationToken ct = default); 
        Task<IEnumerable<FileItem>> GetByOwnerAsync(Guid ownerId, string folderPath, CancellationToken ct = default);
        Task<FileItem> AddAsync(FileItem file, CancellationToken ct = default);
        Task UpdateAsync(FileItem file, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid onwerId, string path, CancellationToken ct = default);
        Task<long> GetTotalSizeByOwnerAsync(Guid ownerId, CancellationToken ct = default);
        Task<FileItem?> GetByContentHashAsync(string hash, CancellationToken ct = default);
        Task<int> GetStorageKeyReferenceCountAsync(string storageKey, CancellationToken ct = default);
        Task HardDeleteAsync(Guid id, CancellationToken ct = default);
    }
}
