using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduleAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalScheduleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CoordinatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByAdminId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleAssignments_users_AssignedByAdminId",
                        column: x => x.AssignedByAdminId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleAssignments_users_CoordinatorId",
                        column: x => x.CoordinatorId,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_ScheduleAssignments_ExternalScheduleId",
                table: "ScheduleAssignments",
                column: "ExternalScheduleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleAssignments_AssignedByAdminId",
                table: "ScheduleAssignments",
                column: "AssignedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleAssignments_CoordinatorId",
                table: "ScheduleAssignments",
                column: "CoordinatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleAssignments");
        }
    }
}