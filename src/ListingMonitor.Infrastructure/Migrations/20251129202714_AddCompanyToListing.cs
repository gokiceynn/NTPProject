using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListingMonitor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyToListing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Listings",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Company",
                table: "Listings");
        }
    }
}
