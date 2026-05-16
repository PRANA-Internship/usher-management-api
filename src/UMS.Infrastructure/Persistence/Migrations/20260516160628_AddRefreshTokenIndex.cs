using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UMS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IDX_Users_RefreshToken",
                table: "users",
                column: "refresh_token",
                unique: true,
                filter: "refresh_token IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IDX_Users_RefreshToken",
                table: "users");
        }
    }
}
