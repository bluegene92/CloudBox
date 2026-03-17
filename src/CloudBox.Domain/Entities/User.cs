using CloudBox.Domain.Common;
using CloudBox.Domain.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Domain.Entities
{
    public class User : Entity
    {
        public string Email { get; private set; } = default!;
        public string DisplayName { get; private set; } = default!;
        public string PasswordHash { get; private set; } = default!;    
        public long StorageQuotaBytes { get; private set; }
        public long StorageUsedBytes { get; private set; }
    
        private User() { }

        public static Result<User> Create(string email, 
            string displayName, 
            string passwordHash,
            long quotaBytes = 5_368_709_120L)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) return Result.Failure<User>("Invalid email address.");

            var user = new User()
            {
                Email = email.ToLowerInvariant().Trim(),
                DisplayName = displayName.Trim(),
                PasswordHash = passwordHash.Trim(),
                StorageQuotaBytes = quotaBytes,
            };


            user.RaiseDomainEvent(new UserRegisteredEvent(user.Id));
            return Result.Success<User>(user);
        }

        public Result ConsumeStorage(long bytes)
        {
            if (StorageUsedBytes + bytes > StorageQuotaBytes)
                return Result.Failure("Storage quota exceeded.");

            StorageUsedBytes += bytes;
            UpdatedAt = DateTime.UtcNow;
            return Result.Success();
        }

        public void ReleaseStorage(long bytes)
        {
            StorageUsedBytes = Math.Max(0, StorageUsedBytes - bytes);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public class UserRegisteredEvent : IDomainEvent
    {
        private Guid _userId { get; }

        public UserRegisteredEvent(Guid userId)
        {
            _userId = userId;
        }
    }
}
