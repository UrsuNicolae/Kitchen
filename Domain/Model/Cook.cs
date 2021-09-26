using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Model
{
    public sealed class Cook
    {
        public Guid Id { get; set; }

        public Order PrepareOrder(Order order)
        {
            return order;
        }
    }
}
