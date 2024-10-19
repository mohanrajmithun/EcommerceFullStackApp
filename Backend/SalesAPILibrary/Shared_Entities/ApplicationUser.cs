using Microsoft.AspNetCore.Identity;
using SalesAPILibrary.Shared_Enums;

namespace SalesAPILibrary.Shared_Entities
{
    public class ApplicationUser:IdentityUser
    {
        public Role Role { get; set; }

    }
}
