using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsherScheduleApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsherScheduleApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalScheduleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UsherId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduleStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduleEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsherScheduleApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsherScheduleApplications_ushers_UsherId",
                        column: x => x.UsherId,
                        principalTable: "ushers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_UsherScheduleApplications_Availability",
                table: "UsherScheduleApplications",
                columns: new[] { "UsherId", "Status", "ScheduleStartDate", "ScheduleEndDate" });

            migrationBuilder.CreateIndex(
                name: "IDX_UsherScheduleApplications_Schedule_Usher",
                table: "UsherScheduleApplications",
                columns: new[] { "ExternalScheduleId", "UsherId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsherScheduleApplications");
        }
    }
}
