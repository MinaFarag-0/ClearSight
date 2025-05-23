using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearSight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDisplayNameToFullName_inUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "Users",
                newName: "FullName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Users",
                newName: "DisplayName");
        }
    }
}
