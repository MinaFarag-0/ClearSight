using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearSight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingDaysOff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "DaysOff",
                table: "Doctors",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DaysOff",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }
    }
}
