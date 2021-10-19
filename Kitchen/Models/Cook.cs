using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kitchen.Models
{
    public sealed class Cook
    {
        public Guid Id { get;}

        public bool IsAvailable { get; set; }
    }
}
