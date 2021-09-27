using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Model
{
    public sealed class Cook
    {
        public Cook()
        {
            Id = new Guid();
            IsAvailable = true;
        }

        public Guid Id { get;}

        public bool IsAvailable { get; set; }

        public Order PrepareOrder(Order order)
        {
            IsAvailable = false;
            Thread.Sleep(100);
            IsAvailable = true;
            return order;
        }
    }
}
