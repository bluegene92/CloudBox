using CloudBox.Domain.Common;
using CloudBox.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudBox.Application.Interfaces
{
    public interface IUserRepository
    {

        Task<User> GetUserByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<User>> GetUsersAsync(CancellationToken ct = default);  
        Task UpdateAsync(User user, CancellationToken ct = default);
        Task SoftDelete(Guid id, CancellationToken ct = default);
        
    }
}
