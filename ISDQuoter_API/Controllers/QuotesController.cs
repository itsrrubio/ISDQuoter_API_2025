using ISDQuoter_API.Data;
using ISDQuoter_API.Dtos;
using ISDQuoter_API.Models;
using ISDQuoter_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Runtime.ConstrainedExecution;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ISDQuoter_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotesController : ControllerBase
    {
        private readonly IQuoteService _quoteService;

        public QuotesController(IQuoteService quoteService)
        {
            _quoteService = quoteService;
        }

        // GET: api/quotes
        [HttpGet]
        public async Task<ActionResult<List<JobQuoteDto>>> GetQuotes()
        {
            var quotes = await _quoteService.GetAllQuotesAsync();
            return Ok(quotes);
        }

        // GET: api/quotes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobQuoteDto>> GetQuote(int id)
        {
            var quote = await _quoteService.GetQuoteByIdAsync(id);

            if (quote == null)
                return NotFound();

            var dto = new JobQuoteDto
            {
                QuoteId = quote.QuoteId,
                GarmentName = quote.Garment?.Name,
                FinalPiecePrice = quote.FinalPiecePrice,
                TotalQuotePrice = quote.TotalQuotePrice,
                Quantity = quote.GarmentQuantity,
                Graphics = quote.Graphics.Select(g => new JobGraphicDto
                {
                    ColorCount = g.ColorCount
                }).ToList()
            };

            return Ok(dto);
        }

        /* NOTE:
        We’ll now update the POST method to:
        Accept the DTO
        Perform the pricing calculation using your rules:
        Screen setup charge: $8 * num_colors / quantity
        Price per item from PrintChargeMatrix
        Underbase: $0.15 * color count
        Garment base price(from Product)
        Fixed markup: $3.00
        */

        [HttpPost]
        public async Task<ActionResult<JobQuoteDto>> CreateQuote(JobQuoteCreateDto dto)
        {
            var (quote, error) = await _quoteService.CreateQuoteAsync(dto);

            if (quote == null)
                return BadRequest(error);

            var garment = quote.Garment;

            var resultDto = new JobQuoteDto
            {
                QuoteId = quote.QuoteId,
                GarmentName = garment?.Name,
                FinalPiecePrice = quote.FinalPiecePrice,
                TotalQuotePrice = quote.TotalQuotePrice,
                Quantity = quote.GarmentQuantity,
                Graphics = quote.Graphics.Select(g => new JobGraphicDto
                {
                    ColorCount = g.ColorCount
                }).ToList()
            };

            return CreatedAtAction(nameof(GetQuote), new { id = quote.QuoteId }, resultDto);
        }
    }
}
