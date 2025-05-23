using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClearSight.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingConfidenceToPatientHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Confidence",
                table: "PatientHistories",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Confidence",
                table: "PatientHistories");
        }
    }
}
