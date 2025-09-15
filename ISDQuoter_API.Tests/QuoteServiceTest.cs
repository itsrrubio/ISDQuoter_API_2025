using ISDQuoter_API.Data;
using ISDQuoter_API.Dtos;
using ISDQuoter_API.Models;
using ISDQuoter_API.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ISDQuoter_API.Tests
{
    public class QuoteServiceTests
    {
        private async Task<AppDbContext> GetInMemoryDbContextAsync()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .EnableSensitiveDataLogging()
                .Options;

            var context = new AppDbContext(options);

            // Seed required data
            context.Products.Add(new Product
            {
                GarmentId = "ABC123",
                Name = "Basic Tee",
                Description = "Short sleeve cotton tee", // Add this line 
                BasePrice = 5.00m
            });

            context.PrintChargeMatrix.Add(new PrintChargeMatrix
            {
                //PrintChargeMatrixId = 1,
                Id = 1,
                MinQty = 1,
                MaxQty = 100,
                ColorQty = 2,
                PricePerItem = 1.25m,
                IsAvailable = true
            });

            await context.SaveChangesAsync();
            return context;
        }

        [Fact]
        public async Task CreateQuoteAsync_ValidDto_ReturnsQuoteWithCorrectPricing()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var service = new QuoteService(context);

            var dto = new JobQuoteCreateDto
            {
                GarmentId = "ABC123",
                GarmentQuantity = 10,
                Graphics = new List<JobGraphicCreateDto>
                {
                    new JobGraphicCreateDto { ColorCount = 2 }
                }
            };

            // Act
            var (quote, error) = await service.CreateQuoteAsync(dto);

            // Assert
            Assert.Null(error);
            Assert.NotNull(quote);
            Assert.Equal("ABC123", quote.GarmentId);
            Assert.Equal(10, quote.GarmentQuantity);

            // Pricing breakdown:
            // Setup: (2 colors * $8) / 10 = 1.60
            // Print Charge = 1.25
            // Underbase = 2 * 0.15 = 0.30
            // Base Price = 5.00
            // Markup = 3.00
            // => Final Piece Price = 1.60 + 1.25 + 0.30 + 5.00 + 3.00 = 11.15

            Assert.Equal(11.15m, quote.FinalPiecePrice);
            Assert.Equal(111.50m, quote.TotalQuotePrice);
        }

        [Fact]
        public async Task CreateQuoteAsync_InvalidGarmentId_ReturnsError()
        {
            var context = await GetInMemoryDbContextAsync(); // No products added
            var service = new QuoteService(context);

            var dto = new JobQuoteCreateDto
            {
                GarmentId = "999",
                GarmentQuantity = 10,
                Graphics = new List<JobGraphicCreateDto> { new JobGraphicCreateDto { ColorCount = 2 } }
            };

            var (quote, error) = await service.CreateQuoteAsync(dto);

            Assert.Null(quote);
            Assert.Equal("Garment ID not found.", error);
        }

        [Fact]
        public async Task CreateQuoteAsync_NoGraphics_ReturnsError()
        {
            var context = await GetInMemoryDbContextAsync();
            var service = new QuoteService(context);

            var dto = new JobQuoteCreateDto
            {
                GarmentId = "ABC123",
                GarmentQuantity = 10,
                Graphics = new List<JobGraphicCreateDto>() // Empty list
            };

            var (quote, error) = await service.CreateQuoteAsync(dto);

            Assert.Null(quote);
            Assert.Equal("At least one graphic is required.", error);
        }

        [Fact]
        public async Task CreateQuoteAsync_MissingPrintChargeMatrix_ReturnsError()
        {
            var context = await GetInMemoryDbContextAsync();

            // Seed product only, skip matrix
            context.Products.Add(new Product
            {
                GarmentId = "EFG456",
                Name = "Basic Tee",
                Description = "Short sleeve cotton tee", // Add this line 
                BasePrice = 5.00m
            });

            await context.SaveChangesAsync();

            var service = new QuoteService(context);

            var dto = new JobQuoteCreateDto
            {
                GarmentId = "ABC123",
                GarmentQuantity = 999, // Will not match matrix (since we didn't add one)
                Graphics = new List<JobGraphicCreateDto>
                {
                    new JobGraphicCreateDto { ColorCount = 2 }
                }
            };

            var (quote, error) = await service.CreateQuoteAsync(dto);

            Assert.Null(quote);
            Assert.Contains("No print charge matrix found", error);
        }

        [Fact]
        public async Task GetQuoteByIdAsync_ExistingId_ReturnsQuoteWithGarmentAndGraphics()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            using var context = new AppDbContext(options);

            // Seed Garment
            var garment = new Product
            {
                GarmentId = "TSHIRT001",
                Name = "T-Shirt",
                Description = "Short sleeve cotton tee", // Add this line 
                BasePrice = 4.00m
            };

            context.Products.Add(garment);

            // Seed JobQuote
            var jobQuote = new JobQuote
            {
                QuoteId = 123,
                GarmentId = "TSHIRT001",
                GarmentQuantity = 20,
                Markup = 3.00m,
                FinalPiecePrice = 10.00m,
                TotalQuotePrice = 200.00m,
                DateCreated = DateTime.UtcNow,
                Graphics = new List<JobGraphic>
        {
            new JobGraphic { ColorCount = 2 }
        }
            };

            context.JobQuotes.Add(jobQuote);
            await context.SaveChangesAsync();

            var service = new QuoteService(context);

            // Act
            var result = await service.GetQuoteByIdAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TSHIRT001", result.GarmentId);
            Assert.Equal(20, result.GarmentQuantity);
            Assert.NotNull(result.Garment);
            Assert.Equal("T-Shirt", result.Garment.Name);
            Assert.Single(result.Graphics);
            Assert.Equal(2, result.Graphics.First().ColorCount);
        }

        [Fact]
        public async Task GetQuoteByIdAsync_NonExistentId_ReturnsNull()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync();
            var service = new QuoteService(context);

            // Act
            var result = await service.GetQuoteByIdAsync(999); // ID not in DB

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllQuotesAsync_QuotesExist_ReturnsMappedDtos()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{System.Guid.NewGuid()}")
                .Options;

            using var context = new AppDbContext(options);

            var product = new Product
            {
                GarmentId = "TSHIRT001",
                Name = "T-Shirt",
                Description = "Short sleeve cotton tee", // Add this line 
                BasePrice = 4.00m
            };

            context.Products.Add(product);

            context.JobQuotes.Add(new JobQuote
            {
                QuoteId = 1,
                GarmentId = "TSHIRT001",
                GarmentQuantity = 25,
                FinalPiecePrice = 10.00m,
                TotalQuotePrice = 250.00m,
                Markup = 3.00m,
                DateCreated = DateTime.UtcNow,
                Graphics = new List<JobGraphic>
        {
            new JobGraphic { ColorCount = 3 }
        }
            });

            context.JobQuotes.Add(new JobQuote
            {
                QuoteId = 2,
                GarmentId = "TSHIRT001",
                GarmentQuantity = 10,
                FinalPiecePrice = 9.00m,
                TotalQuotePrice = 90.00m,
                Markup = 3.00m,
                DateCreated = DateTime.UtcNow,
                Graphics = new List<JobGraphic>
        {
            new JobGraphic { ColorCount = 1 }
        }
            });

            await context.SaveChangesAsync();
            var service = new QuoteService(context);

            // Act
            var result = await service.GetAllQuotesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var first = result.First();
            Assert.Equal(1, first.QuoteId);
            Assert.Equal("T-Shirt", first.GarmentName);
            Assert.Equal(10.00m, first.FinalPiecePrice);
            Assert.Equal(250.00m, first.TotalQuotePrice);
            Assert.Equal(25, first.Quantity);
            Assert.Single(first.Graphics);
            Assert.Equal(3, first.Graphics.First().ColorCount);
        }

        [Fact]
        public async Task GetAllQuotesAsync_NoQuotesExist_ReturnsEmptyList()
        {
            // Arrange
            var context = await GetInMemoryDbContextAsync(); // From earlier setup
            var service = new QuoteService(context);

            // Act
            var result = await service.GetAllQuotesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

    }
}
