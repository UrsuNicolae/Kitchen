using System;

namespace Kitchen.Models
{
    public class OrderFood
    {
        public Food Food { get; set; }

        public bool IsPreparing { get; set; } = false;

        public Guid OrderId { get; set; }
    }
}
