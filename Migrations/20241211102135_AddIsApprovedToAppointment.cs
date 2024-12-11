using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kuaforBerberOtomasyon.Migrations
{
    /// <inheritdoc />
    public partial class AddIsApprovedToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Appointments");
        }
    }
}
