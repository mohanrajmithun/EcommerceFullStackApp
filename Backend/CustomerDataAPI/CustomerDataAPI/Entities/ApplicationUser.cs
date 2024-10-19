using CustomerDataAPI.Enums;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace CustomerDataAPI.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Role Role { get; set; }

        public int? CustomerId { get; set; } // Foreign key from Customer table (nullable if needed)


    }
}

