using CloudBox.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Domain.Entities
{
    public class ShareLink : Entity
    {
        public Guid FileItemId { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public string Token { get; private set; } = default!;
        public SharePermission Permission { get; private set; } 
        public DateTime? ExpiresAt { get; private set; }    
        public int AccessCount { get; private set; }    
        public bool IsRevoked { get; private set; } 

        private ShareLink() { }
        public static ShareLink Create(Guid fileItemId, 
            Guid userId, 
            SharePermission permission, 
            DateTime? expiresAt = null)
        {
            return new ShareLink
            {
                FileItemId = fileItemId,
                CreatedByUserId = userId,
                Token = GenerateToken(),
                Permission = permission,
                ExpiresAt = expiresAt
            };
        }

        public Result RecordAccess()
        {
            if (IsRevoked) return Result.Failure("Share link has been revoked.");
            if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow) return Result.Failure("Share link has expired.");
            AccessCount++;
            return Result.Success();
        }

        public void Revoke()
        {
            IsRevoked = true;
            UpdatedAt = DateTime.UtcNow;
        }

        private static string GenerateToken()
        {
            var bytes = new byte[32];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }

    public enum SharePermission { View, Download, Edit }
}
