using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingEventFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PendingEventId",
                table: "ushers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingScheduleId",
                table: "ushers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingEventId",
                table: "ushers");

            migrationBuilder.DropColumn(
                name: "PendingScheduleId",
                table: "ushers");
        }
    }
}
