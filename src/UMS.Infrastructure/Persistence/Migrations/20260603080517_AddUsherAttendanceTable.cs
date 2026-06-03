using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsherAttendanceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsherAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalScheduleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UsherId = table.Column<Guid>(type: "uuid", nullable: false),
                    MarkedByCoordinatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DayStatus = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsMarked = table.Column<bool>(type: "boolean", nullable: false),
                    MarkedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsherAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsherAttendances_ushers_UsherId",
                        column: x => x.UsherId,
                        principalTable: "ushers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IDX_Attendance_Schedule_Date_Session",
                table: "UsherAttendances",
                columns: new[] { "ExternalScheduleId", "AttendanceDate", "DayStatus" });

            migrationBuilder.CreateIndex(
                name: "IDX_Attendance_Schedule_Usher_Date_Session",
                table: "UsherAttendances",
                columns: new[] { "ExternalScheduleId", "UsherId", "AttendanceDate", "DayStatus" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsherAttendances_UsherId",
                table: "UsherAttendances",
                column: "UsherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsherAttendances");
        }
    }
}
