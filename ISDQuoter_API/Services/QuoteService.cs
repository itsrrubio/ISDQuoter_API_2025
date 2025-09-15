using ISDQuoter_API.Data;
using ISDQuoter_API.Dtos;
using ISDQuoter_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISDQuoter_API.Services
{
    public class QuoteService : IQuoteService
    {
        private readonly AppDbContext _context;

        public QuoteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(JobQuote quote, string error)> CreateQuoteAsync(JobQuoteCreateDto dto)
        {
            // Validate garment
            var garment = await _context.Products.FindAsync(dto.GarmentId);
            if (garment == null)
                return (null, "Garment ID not found.");

            if (dto.Graphics == null || dto.Graphics.Count == 0)
                return (null, "At least one graphic is required.");

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

                // Setup charge per piece = (colorCount * $8) / quantity
                decimal setupChargePerPiece = (colorCount * 8m) / dto.GarmentQuantity;

                // Print charge per item from matrix
                var matrixRow = await _context.PrintChargeMatrix.FirstOrDefaultAsync(p =>
                    p.MinQty <= dto.GarmentQuantity &&
                    p.MaxQty >= dto.GarmentQuantity &&
                    p.ColorQty == colorCount &&
                    p.IsAvailable);

                if (matrixRow == null)
                    return (null, $"No print charge matrix found for {colorCount} color(s) and quantity {dto.GarmentQuantity}.");

                decimal printCharge = matrixRow.PricePerItem;

                // White underbase charge
                decimal underbaseCharge = colorCount * 0.15m;

                // Sum for this graphic
                totalPerPiecePrice += setupChargePerPiece + printCharge + underbaseCharge;

                jobQuote.Graphics.Add(new JobGraphic
                {
                    ColorCount = colorCount
                });
            }

            // Add base garment price and markup
            totalPerPiecePrice += garment.BasePrice + jobQuote.Markup;

            // Store calculated prices
            jobQuote.FinalPiecePrice = totalPerPiecePrice;
            jobQuote.TotalQuotePrice = totalPerPiecePrice * dto.GarmentQuantity;

            // Save to DB
            _context.JobQuotes.Add(jobQuote);
            await _context.SaveChangesAsync();

            return (jobQuote, null);
        }

        public async Task<JobQuote> GetQuoteByIdAsync(int id)
        {
            return await _context.JobQuotes
                .Include(q => q.Garment)
                .Include(q => q.Graphics)
                .FirstOrDefaultAsync(q => q.QuoteId == id);
        }

        public async Task<List<JobQuoteDto>> GetAllQuotesAsync()
        {
            return await _context.JobQuotes
                .Include(q => q.Garment)
                .Include(q => q.Graphics)
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
                .ToListAsync();
        }

    }
}
