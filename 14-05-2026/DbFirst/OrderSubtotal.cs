using System;
using System.Collections.Generic;

namespace DbFirst;

public partial class OrderSubtotal
{
    public int? Orderid { get; set; }

    public decimal? Subtotal { get; set; }
}
