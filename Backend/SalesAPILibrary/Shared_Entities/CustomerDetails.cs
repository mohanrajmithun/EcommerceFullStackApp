﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Shared_Entities
{
    public class CustomerDetails
    {
        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string? PhoneNo { get; set; }

        public bool IsActive { get; set; }
    }
}
