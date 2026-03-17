using CloudBox.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Domain.ValueObjects
{
    public sealed class FilePath : IEquatable<FilePath>
    {
        public string Value { get; }
        private FilePath(string value) => Value = value;
        public string FileName => Value.Split('/').Last();
        public string Directory => Value.Contains('/')
            ? Value[..Value.LastIndexOf('/')]
            : string.Empty;

        public static Result<FilePath> Create(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Result.Failure<FilePath>("File path cannot be empty.");
            }

            var normalized = path.Replace('\\', '/').TrimStart('/');

            if (normalized.Contains(".."))
                return Result.Failure<FilePath>("Path traversal is not allowed.");


            if (normalized.Length > 1024)
                return Result.Failure<FilePath>("File path exceeds maximum length.");

            return Result.Success(new FilePath(normalized));
        }



        public bool Equals(FilePath? other)
        {
            return other is not null && Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            return obj is FilePath fp && Equals(fp);
        }

        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;

    }
}
