using CustomerDataAPI.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CustomerDataAPI.Entities
{
    public class RegistrationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string username { get; set; }

        [Required]
        public string password { get; set; }
        [ValidateNever]
        [JsonIgnore]
        public Role Role { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public string Phone_Number { get; set; }    
    }
}
