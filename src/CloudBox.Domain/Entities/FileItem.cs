using CloudBox.Domain.Common;
using CloudBox.Domain.Events;
using CloudBox.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Domain.Entities
{
    public class FileItem : Entity
    {
        public Guid OwnerId { get; private set; }
        public FilePath Path { get; private set; } = default!;
        public FileSize Size { get; private set; } = default!;
        public string ContentType { get; private set; } = default!;
        public string StorageKey { get; private set; } = default!; // SeaweedFS fid
        public string? ContentHash { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? DeletedAt { get; private set; }
        public int Version { get; private set; }

        private FileItem() { }

        public Result<FileItem> Create(
            Guid ownerId,
            FilePath path,
            FileSize size,
            string contentType,
            string storageKey,
            string? contentHash = null
            )
        {
            if (ownerId == Guid.Empty)
                return Result.Failure<FileItem>("Owner ID is required.");

            var file = new FileItem
            {
                OwnerId = ownerId,
                Path = path,
                Size = size,
                ContentType = contentType,
                StorageKey = storageKey,
                ContentHash = contentHash
            };


            file.RaiseDomainEvent(new FileUploadedEvent(file.Id));

            return Result.Success(file);
        }

        public Result SoftDelete()
        {
            if (IsDeleted) return Result.Failure("File is already deleted.");
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            RaiseDomainEvent(new FileDeletedEvent(Id, OwnerId));
            return Result.Success();
        }

        public void IncrementVersion()
        {
            Version++;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public class FileDeletedEvent : IDomainEvent
    {
        private Guid _fileId { get; }
        private Guid _ownerId { get; }


        public FileDeletedEvent(Guid fileId, Guid onwerId)
        {
            _fileId = fileId;
            _ownerId = onwerId;
        }
    }

    public class FileUploadedEvent: IDomainEvent
    {
        private Guid _fileId { get; }
        public FileUploadedEvent(Guid fileId)
        {
            _fileId = fileId;
        }
    }
}