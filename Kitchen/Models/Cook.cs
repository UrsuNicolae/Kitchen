using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kitchen.Models
{
    public sealed class Cook
    {
        [Key]
        public int Id { get; set; }

        public bool IsAvailable { get; set; }
    }
}
