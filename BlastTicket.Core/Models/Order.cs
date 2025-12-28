using System;
using System.Collections.Generic;
using System.Text;

namespace BlastTicket.Core.Models
{
    public record struct Order(
        Guid Id,
        Guid ItemId,
        Guid UserId,
        int Quantity,
        DateTime CreatedAt
        );
}
