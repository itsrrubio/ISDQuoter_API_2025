namespace ISDQuoter_API.Dtos
{
    public class JobQuoteDto
    {
        public int QuoteId { get; set; }
        public string GarmentName { get; set; }
        public decimal? FinalPiecePrice { get; set; }
        public decimal? TotalQuotePrice { get; set; }
        public int Quantity { get; set; }
        public List<JobGraphicDto> Graphics { get; set; }
    }
}
