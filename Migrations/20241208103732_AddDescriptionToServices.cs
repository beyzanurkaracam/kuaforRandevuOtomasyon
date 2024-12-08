using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kuaforBerberOtomasyon.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ServiceID",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "Employees",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "Employees");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Employees",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "ServiceID",
                table: "Employees",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Employees",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
