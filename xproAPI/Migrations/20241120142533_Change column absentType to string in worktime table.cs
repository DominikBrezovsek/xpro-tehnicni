using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xproAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangecolumnabsentTypetostringinworktimetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AbsentType",
                table: "WorkTimes",
                type: "text",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte[]>(
                name: "ProfileImage",
                table: "Users",
                type: "bytea",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "AbsentType",
                table: "WorkTimes",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProfileImage",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "bytea",
                oldNullable: true);
        }
    }
}
