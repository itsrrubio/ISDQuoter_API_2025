using ISDQuoter_API.Controllers;
using ISDQuoter_API.Dtos;
using ISDQuoter_API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISDQuoter_API.Tests
{
    public class QuotesControllerTests
    {
        private readonly Mock<IQuoteService> _mockQuoteService;
        private readonly QuotesController _controller;

        public QuotesControllerTests()
        {
            _mockQuoteService = new Mock<IQuoteService>();
            _controller = new QuotesController(_mockQuoteService.Object);
        }

        [Fact]
        public async Task GetQuotes_ReturnsOkResult_WithListOfQuotes()
        {
            // Arrange
            var mockQuotes = new List<JobQuoteDto>
            {
                new JobQuoteDto { QuoteId = 1, GarmentName = "T-Shirt", FinalPiecePrice = 12.50m, Quantity = 50 },
                new JobQuoteDto { QuoteId = 2, GarmentName = "Hoodie", FinalPiecePrice = 22.75m, Quantity = 30 }
            };

            _mockQuoteService.Setup(service => service.GetAllQuotesAsync())
                             .ReturnsAsync(mockQuotes);

            // Act
            var result = await _controller.GetQuotes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnQuotes = Assert.IsAssignableFrom<List<JobQuoteDto>>(okResult.Value);
            Assert.Equal(2, returnQuotes.Count);
        }
    }
}
