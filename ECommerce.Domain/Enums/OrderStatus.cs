using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,
    Paid = 2,
    Processing = 3,
    Shipped = 4,
    Delivered = 5,
    Cancelled = 6
}
