using System;
using System.Collections.Generic;
using System.Text;
using UMS.Domain.Entities;

namespace UMS.Application.Common.Interfaces
{
    public interface IUsherRepository
    {
        Task AddAsync(Usher usher, CancellationToken ct = default);


        //for admin to view ushers with there id
        Task<Usher?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    }
}
