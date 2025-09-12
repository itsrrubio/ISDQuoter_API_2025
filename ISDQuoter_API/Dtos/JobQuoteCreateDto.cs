namespace ISDQuoter_API.Dtos
{
    public class JobQuoteCreateDto
    {
        public string GarmentId { get; set; }
        public int GarmentQuantity { get; set; }
        public List<JobGraphicCreateDto> Graphics { get; set; }
    }
}
