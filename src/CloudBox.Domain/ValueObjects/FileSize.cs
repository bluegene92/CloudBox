using CloudBox.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Domain.ValueObjects
{
    public sealed class FileSize
    {
        public long Bytes { get; }
        private FileSize(long bytes) => Bytes = bytes;

        public static Result<FileSize> Create(long bytes)
        {
            if (bytes < 0)
                return Result.Failure<FileSize>("File size cannot be negative.");


            if (bytes > 5L * 1024 * 1024 * 1024) // B, MB, GB
                return Result.Failure<FileSize>("File exceed the 5GB size limit.");

            return Result.Success(new FileSize(bytes));
        }

        public override string ToString()
        {
            if (Bytes < 1024) return $"{Bytes} B";
            if (Bytes < 1024 * 1024) return $"{Bytes / 1024.0:F1} KB";
            if (Bytes < 1024L * 1024 * 1024) return $"{Bytes / (1024.0 * 1024):F1} MB";
            return $"{Bytes / (1024.0 * 1024 * 1024):F2} GB";
        }
    }
}
