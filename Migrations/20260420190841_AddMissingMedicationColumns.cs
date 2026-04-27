using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medication_Tracker.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingMedicationColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Medications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TimeToTake",
                table: "Medications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Medications_UserId",
                table: "Medications",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medications_Users_UserId",
                table: "Medications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medications_Users_UserId",
                table: "Medications");

            migrationBuilder.DropIndex(
                name: "IX_Medications_UserId",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "TimeToTake",
                table: "Medications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Medications");
        }
    }
}
