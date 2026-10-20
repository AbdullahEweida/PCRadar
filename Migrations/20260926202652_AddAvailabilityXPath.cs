using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCRadar.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilityXPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityXPath",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailabilityXPath",
                table: "Stores");
        }
    }
}
