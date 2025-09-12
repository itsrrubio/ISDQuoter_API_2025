using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDQuoter_API.Models
{
    public class JobQuote
    {
        [Key]  // <-- Add this line
        public int QuoteId { get; set; }

        [Required]
        public string GarmentId { get; set; }

        [ForeignKey("GarmentId")]
        public Product Garment { get; set; }

        public int GarmentQuantity { get; set; }

        [Column(TypeName = "decimal(6, 2)")]
        public decimal Markup { get; set; } = 3.00m;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(6, 2)")]
        public decimal? FinalPiecePrice { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? TotalQuotePrice { get; set; }

        public ICollection<JobGraphic> Graphics { get; set; }

    }
}
