using ISDQuoter_API.Dtos;
using ISDQuoter_API.Models;
using System.Threading.Tasks;

namespace ISDQuoter_API.Services
{
    public interface IQuoteService
    {
        Task<List<JobQuoteDto>> GetAllQuotesAsync();

        /// <summary>
        /// Retrieves a JobQuote by ID including related entities.
        /// </summary>
        Task<JobQuote> GetQuoteByIdAsync(int id);

        /// <summary>
        /// Creates a new JobQuote based on the provided DTO, performing all pricing calculations.
        /// Returns the created JobQuote or null if there was an error.
        /// </summary>
        /// <param name="dto">Quote creation DTO</param>
        /// <returns>Tuple of JobQuote and error message (null if success)</returns>
        Task<(JobQuote quote, string error)> CreateQuoteAsync(JobQuoteCreateDto dto);
    }
}
