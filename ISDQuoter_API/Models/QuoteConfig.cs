namespace ISDQuoter_API.Models
{
    public class QuoteConfig
    {
        public int Id { get; set; } // Just use 1 record for now
        public decimal SetupChargePerColor { get; set; }
        public decimal UnderbaseChargePerColor { get; set; }
        public decimal Markup { get; set; }
    }
}
