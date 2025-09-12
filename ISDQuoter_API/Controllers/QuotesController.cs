using ISDQuoter_API.Data;
using ISDQuoter_API.Dtos;
using ISDQuoter_API.Models;
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
        private readonly AppDbContext _context;

        public QuotesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/quotes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<JobQuote>>> GetQuotes()
        {
            return await _context.JobQuotes
                .Include(q => q.Garment)
                .Include(q => q.Graphics)
                .ToListAsync();
        }

        // GET: api/quotes/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<JobQuote>> GetQuote(int id)
        //{
        //    var quote = await _context.JobQuotes
        //        .Include(q => q.Garment)
        //        .Include(q => q.Graphics)
        //        .FirstOrDefaultAsync(q => q.QuoteId == id);

        //    if (quote == null)
        //    {
        //        return NotFound();
        //    }

        //    return quote;
        //}

        // GET: api/quotes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<JobQuoteDto>> GetQuote(int id)
        {
            var dto = await _context.JobQuotes
                .Where(q => q.QuoteId == id)
                .Select(quote => new JobQuoteDto
                {
                    QuoteId = quote.QuoteId,
                    GarmentName = quote.Garment.Name,
                    FinalPiecePrice = quote.FinalPiecePrice,
                    TotalQuotePrice = quote.TotalQuotePrice,
                    Quantity = quote.GarmentQuantity,
                    Graphics = quote.Graphics.Select(g => new JobGraphicDto
                    {
                        ColorCount = g.ColorCount
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound();

            return Ok(dto);
        }

        //[HttpGet("{id}")]
        //public async Task<ActionResult<JobQuoteDto>> GetQuote(int id)
        //{
        //    var quote = await _context.JobQuotes
        //        .Include(q => q.Garment)
        //        .Include(q => q.Graphics)
        //        .FirstOrDefaultAsync(q => q.QuoteId == id);

        //    if (quote == null) return NotFound();

        //    var dto = new JobQuoteDto
        //    {
        //        QuoteId = quote.QuoteId,
        //        GarmentName = quote.Garment?.Name,
        //        FinalPiecePrice = quote.FinalPiecePrice,
        //        TotalQuotePrice = quote.TotalQuotePrice,
        //        Quantity = quote.GarmentQuantity,
        //        Graphics = quote.Graphics?.Select(g => new JobGraphicDto
        //        {
        //            ColorCount = g.ColorCount
        //        }).ToList()
        //    };

        //    return Ok(dto);
        //}


        //// POST: api/quotes
        //[HttpPost]
        //public async Task<ActionResult<JobQuote>> CreateQuote(JobQuote quote)
        //{
        //    // You can add validation logic here or in a service class

        //    _context.JobQuotes.Add(quote);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetQuote), new { id = quote.QuoteId }, quote);
        //}

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
        public async Task<ActionResult<JobQuote>> CreateQuote(JobQuoteCreateDto dto)
        {
            // 1. Validate Garment exists
            var garment = await _context.Products.FindAsync(dto.GarmentId);
            if (garment == null)
            {
                return BadRequest("Garment ID not found.");
            }

            if (dto.Graphics == null || dto.Graphics.Count == 0)
            {
                return BadRequest("At least one graphic is required.");
            }

            // 2. Start building the JobQuote
            var jobQuote = new JobQuote
            {
                GarmentId = dto.GarmentId,
                GarmentQuantity = dto.GarmentQuantity,
                Markup = 3.00m,
                DateCreated = DateTime.UtcNow,
                Graphics = new List<JobGraphic>()
            };

            decimal totalPerPiecePrice = 0;

            foreach (var graphicDto in dto.Graphics)
            {
                int colorCount = graphicDto.ColorCount;

                // a. Setup Charge per piece = (colorCount * $8) / quantity
                decimal setupChargePerPiece = (colorCount * 8m) / dto.GarmentQuantity;

                // b. Print charge per item from matrix
                var matrixRow = await _context.PrintChargeMatrix.FirstOrDefaultAsync(p =>
                    p.MinQty <= dto.GarmentQuantity &&
                    p.MaxQty >= dto.GarmentQuantity &&
                    p.ColorQty == colorCount &&
                    p.IsAvailable);

                if (matrixRow == null)
                {
                    return BadRequest($"No print charge matrix found for {colorCount} color(s) and quantity {dto.GarmentQuantity}.");
                }

                decimal printCharge = matrixRow.PricePerItem;

                // c. White underbase charge
                decimal underbaseCharge = colorCount * 0.15m;

                // d. Sum for this graphic
                totalPerPiecePrice += setupChargePerPiece + printCharge + underbaseCharge;

                // e. Add to graphics
                jobQuote.Graphics.Add(new JobGraphic
                {
                    ColorCount = colorCount
                });
            }

            // 3. Add base garment price and markup
            totalPerPiecePrice += garment.BasePrice + jobQuote.Markup;

            // 4. Optionally: store calculated price (if you add properties for this)
            jobQuote.FinalPiecePrice = totalPerPiecePrice;
            jobQuote.TotalQuotePrice = totalPerPiecePrice * dto.GarmentQuantity;

            // 5. Save to DB
            _context.JobQuotes.Add(jobQuote);
            await _context.SaveChangesAsync();

            //return CreatedAtAction(nameof(GetQuote), new { id = jobQuote.QuoteId }, jobQuote);
            return CreatedAtAction(nameof(GetQuote), new { id = jobQuote.QuoteId }, new JobQuoteDto
            {
                QuoteId = jobQuote.QuoteId,
                GarmentName = garment.Name,
                FinalPiecePrice = jobQuote.FinalPiecePrice,
                TotalQuotePrice = jobQuote.TotalQuotePrice,
                Quantity = jobQuote.GarmentQuantity,
                Graphics = jobQuote.Graphics.Select(g => new JobGraphicDto
                {
                    ColorCount = g.ColorCount
                }).ToList()
            });

        }


        // TODO: Add PUT and DELETE methods if needed
    }
}

//namespace ISDQuoter_API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class QuotesController : ControllerBase
//    {

//    }
//}
