using System;

namespace Kitchen.Models
{
    public class OrderWithIds
    {
        public Order Order { get; set; }
        public Guid TableId { get; set; }

        public Guid WaiterId { get; set; }
    }
}
