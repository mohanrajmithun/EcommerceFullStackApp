using System.ComponentModel.DataAnnotations;

namespace SalesAPILibrary.Shared_Entities
{
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }
    }
}
