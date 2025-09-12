using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDQuoter_API.Models
{
    public class Product
    {

        [Key]
        public string GarmentId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal BasePrice { get; set; }

        public ICollection<JobQuote> JobQuotes { get; set; }

    }
}
