using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medication_Tracker.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToMedicationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MedicationLogs");

            migrationBuilder.DropColumn(
                name: "WasTaken",
                table: "MedicationLogs");

            migrationBuilder.RenameColumn(
                name: "TakenAt",
                table: "MedicationLogs",
                newName: "DateTaken");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MedicationLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MedicationLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "MedicationLogs");

            migrationBuilder.RenameColumn(
                name: "DateTaken",
                table: "MedicationLogs",
                newName: "TakenAt");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "MedicationLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MedicationLogs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "WasTaken",
                table: "MedicationLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
