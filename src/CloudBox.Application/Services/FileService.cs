using CloudBox.Application.DTOs;
using CloudBox.Application.Interfaces;
using CloudBox.Domain.Common;
using CloudBox.Domain.Entities;
using CloudBox.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IUniteOfWork _uow;
        private readonly IStorageService _storage;
        private readonly IValidator<UploadFileRequest> _uploadValidator;
        private readonly ILogger<FileService> _logger;

        public FileService(IUniteOfWork uow, 
            IStorageService storage, 
            IValidator<UploadFileRequest> upload,
            ILogger<FileService> logger)
        {
            _uow = uow;
            _storage = storage;
            _uploadValidator = upload;
            _logger = logger;
        }

        public Task<Result> DeleteAsync(Guid userId, Guid fileId, CancellationToken dt = default)
        {
            throw new NotImplementedException();
        }

        public Task<Result<DownloadUrlResponse>> GetDownloadUrlAsync(Guid userId, Guid fileId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<FileItemDto>> GetFileByIdAsync(Guid userId, Guid fileId, CancellationToken ct = default)
        {
            var file = await _uow.Files.GetByIdAsync(fileId, ct);

            if (file == null || file.OwnerId != userId)
                return Result.Failure<FileItemDto>("File not found.");

            return Result.Success(new FileItemDto(
                file.Id,
                file.Path.FileName,
                file.Path.Value,
                file.Size.ToString(),
                file.ContentType,
                file.CreatedAt,
                file.Version    
                ));
        }

        public async Task<Result<IEnumerable<FileItemDto>>> GetFilesAsync(Guid userId, string folderPath, CancellationToken ct = default)
        {
            var files = await _uow.Files.GetByOwnerAsync(userId, folderPath, ct);

            var dtos = files.Where(f => !f.IsDeleted)
                .Select(x => new FileItemDto(
                    x.Id, 
                    x.Path.FileName,
                    x.Path.Value,
                    x.Size.ToString(),
                    x.ContentType,
                    x.CreatedAt,
                    x.Version
                    ))
                .OrderBy(x => x.Name);

            return Result.Success<IEnumerable<FileItemDto>>(dtos);
        }

        public async Task<Result<UploadFileResponse>> UploadAsync(UploadFileRequest request, CancellationToken ct = default)
        {
            // 1. Validate request
            var validation = await _uploadValidator.ValidateAsync(request, ct);

            if (!validation.IsValid)
            {
                var errors = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                return Result.Failure<UploadFileResponse>(errors);  
            }

            // 2. Build domain value objects
            var rawpath = string.IsNullOrWhiteSpace(request.FolderPath)
                ? request.FileName
                : $"{request.FolderPath.TrimEnd('/')}/{request.FileName}";

            var pathResult = FilePath.Create(rawpath);
            if (pathResult.IsFailure)
                return Result.Failure<UploadFileResponse>(pathResult.Error);


            var sizeResult = FileSize.Create(request.ContentLength);
            if (sizeResult.IsFailure)
                return Result.Failure<UploadFileResponse>(sizeResult.Error);

            // 3. Content deduplication via SHA-256
            var (hash, rewindableStream) = await ComputeHashAsync(request.Content, ct);

            var existingFile = await _uow.Files.GetByContentHashAsync(hash, ct);

            // 4. Enforce storage quota at domain level
            var user = await _uow.Users.GetUserByIdAsync(request.UserId, ct);
            if (user is null)
                return Result.Failure<UploadFileResponse>("user not found.");

            var quotaResult = user.ConsumeStorage(request.ContentLength);
            if (quotaResult.IsFailure)
                return Result.Failure<UploadFileResponse>(quotaResult.Error);

            // 5. Upload blob (reuse existing fid on dedup hit)
            string storageKey;
            if (existingFile is not null)
            {
                storageKey = existingFile.StorageKey;
                _logger.LogInformation("Dedup hit for hash {Hash}", hash);
            } else
            {
                try
                {
                    storageKey = await _storage.UploadAsync(rewindableStream, request.ContentType, ct);
                } catch (Exception ex)
                {
                    _logger.LogError(ex, "Blob upload failed for user {UserId}", request.UserId);
                    return Result.Failure<UploadFileResponse>(
                        "File upload failed. Please retry.");

                }
            }

            // 6. Persist metadata in DB transaction
            await _uow.BeginTransactionAsync(ct);
            try
            {
                var fileResult = FileItem.Create(request.UserId,
                pathResult.Value,
                sizeResult.Value,
                request.ContentType,
                storageKey,
                hash);


                if (fileResult.IsFailure)
                {
                    if (existingFile is null)
                        await _storage.DeleteAsync(storageKey, ct);
                    return Result.Failure<UploadFileResponse>(fileResult.Error);
                }

                await _uow.Files.AddAsync(fileResult.Value, ct);
                await _uow.Users.UpdateAsync(user, ct);
                await _uow.CommitAsync(ct);


                _logger.LogInformation("File {FileId} uploaded by user {UserId}", fileResult.Value.Id, request.UserId);

                return Result.Success(new UploadFileResponse(
                    fileResult.Value.Id,
                    pathResult.Value.Value,
                    sizeResult.Value.ToString(),
                    fileResult.Value.CreatedAt
                    ));
            } catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                if (existingFile is null)
                    await _storage.DeleteAsync(storageKey, ct);
                _logger.LogError(ex, "DB write failed for user {UserId}", request.UserId);

                return Result.Failure<UploadFileResponse>("An error occurred saving the file");
            }




        }
    }
}
