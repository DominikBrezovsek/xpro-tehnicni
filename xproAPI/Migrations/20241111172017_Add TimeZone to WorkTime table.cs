using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xproAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeZonetoWorkTimetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "WorkTimes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "WorkTimes");
        }
    }
}
