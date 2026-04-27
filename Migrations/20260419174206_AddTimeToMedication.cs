using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medication_Tracker.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeToMedication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeToTake",
                table: "Medications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeToTake",
                table: "Medications");
        }
    }
}
