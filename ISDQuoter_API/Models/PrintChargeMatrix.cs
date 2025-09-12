using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDQuoter_API.Models
{
    public class PrintChargeMatrix
    {
        [Key]
        public int Id { get; set; }

        public int MinQty { get; set; }

        public int MaxQty { get; set; }

        public int ColorQty { get; set; }

        [Column(TypeName = "decimal(6, 2)")]
        public decimal PricePerItem { get; set; }

        public bool IsAvailable { get; set; } = true;

    }
}
