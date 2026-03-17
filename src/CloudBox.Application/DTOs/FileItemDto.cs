using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Application.DTOs
{
    public record FileItemDto(
        Guid Id,
        string Name,
        string Path,
        string Size,
        string contentType,
        DateTime CreatedAt,
        int Version
    );

    public record UploadFileResponse(
        Guid FileId,
        string Path,
        string Size,
        DateTime CreatedAt
    );

    public record DownloadUrlResponse(string Url);
    public record CreateShareLinkResponse(string Token, string shareUrl);
}
