using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Application.DTOs
{
    public record UploadFileDto();
    public record UploadFileRequest(
        Guid UserId,
        string FileName,
        string FolderPath,
        Stream Content,
        long ContentLength,
        string ContentType
    );
}
