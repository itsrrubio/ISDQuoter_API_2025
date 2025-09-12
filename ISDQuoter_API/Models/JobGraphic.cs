using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ISDQuoter_API.Models
{
    public class JobGraphic
    {
        [Key]
        public int Id { get; set; }

        public int QuoteId { get; set; }

        [ForeignKey("QuoteId")]
        public JobQuote JobQuote { get; set; }

        public int ColorCount { get; set; }

    }
}
