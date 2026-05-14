using System;
using System.Collections.Generic;
using System.Text;

namespace UMS.Application.Common.Interfaces
{

    public interface IUnitOfWork
    {
        Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken ct = default);
    }

    // the above interface we need it because we want to make sure that if any of the operations in the transaction(in registration)- fails,
    // we can roll back the entire transaction to maintain data integrity. 

}


