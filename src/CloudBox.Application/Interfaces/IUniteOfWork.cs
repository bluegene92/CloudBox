using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Application.Interfaces
{
    public interface IUniteOfWork
    {
        IFileRepository Files { get; }

        Task BeginTransactionAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}
