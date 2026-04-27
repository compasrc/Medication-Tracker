using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medication_Tracker.Migrations
{
    /// <inheritdoc />
    public partial class AddWasTakenToMedicationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTaken",
                table: "MedicationLogs");

            migrationBuilder.AlterColumn<string>(
                name: "TimeToTake",
                table: "Medications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "TakenAt",
                table: "MedicationLogs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasTaken",
                table: "MedicationLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TakenAt",
                table: "MedicationLogs");

            migrationBuilder.DropColumn(
                name: "WasTaken",
                table: "MedicationLogs");

            migrationBuilder.AlterColumn<string>(
                name: "TimeToTake",
                table: "Medications",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTaken",
                table: "MedicationLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
