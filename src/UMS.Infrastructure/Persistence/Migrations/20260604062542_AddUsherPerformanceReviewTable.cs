using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsherPerformanceReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsherPerformanceReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalScheduleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UsherId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedByCoordinatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Grooming = table.Column<int>(type: "integer", nullable: false),
                    Professionalism = table.Column<int>(type: "integer", nullable: false),
                    Communication = table.Column<int>(type: "integer", nullable: false),
                    Teamwork = table.Column<int>(type: "integer", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsherPerformanceReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsherPerformanceReviews_ushers_UsherId",
                        column: x => x.UsherId,
                        principalTable: "ushers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_PerformanceReview_Schedule_Usher",
                table: "UsherPerformanceReviews",
                columns: new[] { "ExternalScheduleId", "UsherId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsherPerformanceReviews_UsherId",
                table: "UsherPerformanceReviews",
                column: "UsherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsherPerformanceReviews");
        }
    }
}
