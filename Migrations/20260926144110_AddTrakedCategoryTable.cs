using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PCRadar.Migrations
{
    /// <inheritdoc />
    public partial class AddTrakedCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardXPath",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkXPath",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameXPath",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PriceXPath",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "TrackedCategories",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    CategoryUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedCategories", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrackedCategories_Stores_StoreID",
                        column: x => x.StoreID,
                        principalTable: "Stores",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackedCategories_StoreID",
                table: "TrackedCategories",
                column: "StoreID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackedCategories");

            migrationBuilder.DropColumn(
                name: "CardXPath",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "LinkXPath",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "NameXPath",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "PriceXPath",
                table: "Stores");
        }
    }
}
