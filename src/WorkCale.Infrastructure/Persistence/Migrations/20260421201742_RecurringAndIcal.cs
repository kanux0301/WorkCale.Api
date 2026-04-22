using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkCale.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RecurringAndIcal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnpaidBreakMinutes",
                table: "Shifts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "ShiftCategories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnpaidBreakMinutes",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "ShiftCategories");
        }
    }
}
