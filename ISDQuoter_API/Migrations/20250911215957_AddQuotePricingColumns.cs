using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISDQuoter_API.Migrations
{
    /// <inheritdoc />
    public partial class AddQuotePricingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalPiecePrice",
                table: "JobQuotes",
                type: "decimal(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalQuotePrice",
                table: "JobQuotes",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalPiecePrice",
                table: "JobQuotes");

            migrationBuilder.DropColumn(
                name: "TotalQuotePrice",
                table: "JobQuotes");
        }
    }
}
