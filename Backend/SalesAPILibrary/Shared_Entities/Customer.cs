using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAPILibrary.Shared_Entities
{
    public class Customer
    {
        [Key]

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string? Address { get; set; }

        public string? Email { get; set; }

        public string? PhoneNo { get; set; }

        public bool IsActive { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }






    }
}
