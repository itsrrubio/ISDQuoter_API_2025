using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ISDQuoter_API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrintChargeMatrix",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MinQty = table.Column<int>(type: "int", nullable: false),
                    MaxQty = table.Column<int>(type: "int", nullable: false),
                    ColorQty = table.Column<int>(type: "int", nullable: false),
                    PricePerItem = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrintChargeMatrix", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    GarmentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.GarmentId);
                });

            migrationBuilder.CreateTable(
                name: "JobQuotes",
                columns: table => new
                {
                    QuoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GarmentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GarmentQuantity = table.Column<int>(type: "int", nullable: false),
                    Markup = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobQuotes", x => x.QuoteId);
                    table.ForeignKey(
                        name: "FK_JobQuotes_Products_GarmentId",
                        column: x => x.GarmentId,
                        principalTable: "Products",
                        principalColumn: "GarmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobGraphics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuoteId = table.Column<int>(type: "int", nullable: false),
                    ColorCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobGraphics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobGraphics_JobQuotes_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "JobQuotes",
                        principalColumn: "QuoteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobGraphics_QuoteId",
                table: "JobGraphics",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_JobQuotes_GarmentId",
                table: "JobQuotes",
                column: "GarmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobGraphics");

            migrationBuilder.DropTable(
                name: "PrintChargeMatrix");

            migrationBuilder.DropTable(
                name: "JobQuotes");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
