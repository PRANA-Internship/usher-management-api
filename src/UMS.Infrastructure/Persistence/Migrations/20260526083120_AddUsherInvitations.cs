using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsherInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsherInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalScheduleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UsherId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvitedByCoordinatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    RespondedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ScheduleStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduleEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsherInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsherInvitations_users_InvitedByCoordinatorId",
                        column: x => x.InvitedByCoordinatorId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsherInvitations_ushers_UsherId",
                        column: x => x.UsherId,
                        principalTable: "ushers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_UsherInvitations_Availability",
                table: "UsherInvitations",
                columns: new[] { "UsherId", "Status", "ScheduleStartDate", "ScheduleEndDate" });

            migrationBuilder.CreateIndex(
                name: "IDX_UsherInvitations_Schedule_Usher",
                table: "UsherInvitations",
                columns: new[] { "ExternalScheduleId", "UsherId" });

            migrationBuilder.CreateIndex(
                name: "IX_UsherInvitations_InvitedByCoordinatorId",
                table: "UsherInvitations",
                column: "InvitedByCoordinatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsherInvitations");
        }
    }
}