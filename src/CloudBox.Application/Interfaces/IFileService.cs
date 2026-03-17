using CloudBox.Application.DTOs;
using CloudBox.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Application.Interfaces
{
    public interface IFileService
    {
        Task<Result<IEnumerable<FileItemDto>>>  GetFilesAsync(Guid userId, string folderPath, CancellationToken ct = default);
        Task<Result<FileItemDto>> GetFileByIdAsync(Guid userId, Guid fileId, CancellationToken ct = default);
        Task<Result<UploadFileResponse>> UploadAsync(UploadFileRequest request, CancellationToken ct = default);
        Task<Result> DeleteAsync(Guid userId, Guid fileId, CancellationToken dt = default);
        Task<Result<DownloadUrlResponse>> GetDownloadUrlAsync(Guid userId, Guid fileId, CancellationToken ct = default);
    }
}
