using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkCale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryDefaultTimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultStartTime",
                table: "ShiftCategories",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DefaultEndTime",
                table: "ShiftCategories",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultStartTime",
                table: "ShiftCategories");

            migrationBuilder.DropColumn(
                name: "DefaultEndTime",
                table: "ShiftCategories");
        }
    }
}
