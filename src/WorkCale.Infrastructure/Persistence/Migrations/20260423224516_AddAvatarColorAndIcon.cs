using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkCale.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarColorAndIcon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarColor",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarIcon",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarColor",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarIcon",
                table: "Users");
        }
    }
}
