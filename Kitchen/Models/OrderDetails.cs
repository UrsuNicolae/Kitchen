using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kitchen.Models
{
    public class OrderDetails
    {
        [Required]
        public Guid Id { get; set; }

        public Guid TableId { get; set; }

        public Guid WaiterId { get; set; }

        public IList<int> Items { get; set; } = new List<int>();

        [Required]
        [Range(1, 5)]
        public byte Priority { get; set; }

        public DateTime PickUpTime { get; set; }

        [Required]
        public double MaxWaitTime { get; set; }

        [Required]
        public ThreadSafeListWithLock<(int, int)> CookingDetails {get; set;} = new ThreadSafeListWithLock<(int, int)> { };
    }
}
