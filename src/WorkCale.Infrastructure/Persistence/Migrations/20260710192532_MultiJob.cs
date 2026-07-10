using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkCale.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MultiJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create Jobs table (no cross-FKs yet).
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 2. Seed one default Job per existing user so we have something to backfill FKs to.
            migrationBuilder.Sql("""
                INSERT INTO "Jobs" ("Id", "UserId", "Name", "Color", "Icon", "IsDefault", "IsArchived", "SortOrder", "CreatedAt", "UpdatedAt")
                SELECT gen_random_uuid(), u."Id", 'My Job', '#4C6FA3', 'briefcase-outline', TRUE, FALSE, 0, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC'
                FROM "Users" u;
                """);

            // 3. Add JobId columns as NULLABLE first so the ALTER succeeds.
            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "Shifts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "ShiftCategories",
                type: "uuid",
                nullable: true);

            // 4. Backfill JobId from each user's default Job.
            migrationBuilder.Sql("""
                UPDATE "Shifts" s
                SET "JobId" = j."Id"
                FROM "Jobs" j
                WHERE j."UserId" = s."UserId" AND j."IsDefault" = TRUE;
                """);

            migrationBuilder.Sql("""
                UPDATE "ShiftCategories" c
                SET "JobId" = j."Id"
                FROM "Jobs" j
                WHERE j."UserId" = c."UserId" AND j."IsDefault" = TRUE;
                """);

            // 5. Now that all rows have a valid JobId, make the columns NOT NULL.
            migrationBuilder.AlterColumn<Guid>(
                name: "JobId",
                table: "Shifts",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "JobId",
                table: "ShiftCategories",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // 6. Indices.
            migrationBuilder.CreateIndex(
                name: "IX_Shifts_JobId",
                table: "Shifts",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_UserId_JobId_Date",
                table: "Shifts",
                columns: new[] { "UserId", "JobId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftCategories_JobId",
                table: "ShiftCategories",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftCategories_UserId_JobId",
                table: "ShiftCategories",
                columns: new[] { "UserId", "JobId" });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_UserId_DefaultUnique",
                table: "Jobs",
                column: "UserId",
                unique: true,
                filter: "\"IsDefault\" = TRUE");

            // 7. Cross-FKs now that the data is valid.
            migrationBuilder.AddForeignKey(
                name: "FK_ShiftCategories_Jobs_JobId",
                table: "ShiftCategories",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shifts_Jobs_JobId",
                table: "Shifts",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShiftCategories_Jobs_JobId",
                table: "ShiftCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Shifts_Jobs_JobId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_JobId",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Shifts_UserId_JobId_Date",
                table: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_ShiftCategories_JobId",
                table: "ShiftCategories");

            migrationBuilder.DropIndex(
                name: "IX_ShiftCategories_UserId_JobId",
                table: "ShiftCategories");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "Shifts");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "ShiftCategories");

            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
