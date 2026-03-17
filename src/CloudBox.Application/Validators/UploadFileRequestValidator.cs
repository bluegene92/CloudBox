using CloudBox.Application.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Application.Validators
{
    public class UploadFileRequestValidator : AbstractValidator<UploadFileRequest>
    {
        private static readonly HashSet<string> AllowedTypes =
        new(StringComparer.OrdinalIgnoreCase)
{
        "image/jpeg", "image/png", "image/gif", "image/webp",
        "application/pdf", "text/plain", "text/csv",
        "application/zip", "video/mp4", "audio/mpeg"
};

        public UploadFileRequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.FileName)
                .NotEmpty()
                .MaximumLength(255)
                .Must(n => !n.Contains('/') && !n.Contains('\\'))
                .WithMessage("File name cannot contain path separates.");
            RuleFor(x => x.ContentLength)
                .GreaterThan(0)
                .WithMessage("File cannot be empty.")
                .LessThanOrEqualTo(5L * 1024 * 1024 * 1024)
                .WithMessage("File cannot exceed 5 GB.");
            RuleFor(x => x.ContentType)
                .Must(ct => AllowedTypes.Contains(ct))
                .WithMessage("File type is not permitted.");
        }

    }
}
