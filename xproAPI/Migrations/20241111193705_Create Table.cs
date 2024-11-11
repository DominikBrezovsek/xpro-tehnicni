using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace xproAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedBreakTime",
                table: "WorkTimes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WorkTimes",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WorkTimes",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<long>(
                name: "BreakDurationId",
                table: "WorkTimes",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "BreakDurations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Duration = table.Column<int>(type: "integer", nullable: false),
                    Valid = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakDurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkTimes_BreakDurationId",
                table: "WorkTimes",
                column: "BreakDurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkTimes_BreakDurations_BreakDurationId",
                table: "WorkTimes",
                column: "BreakDurationId",
                principalTable: "BreakDurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkTimes_BreakDurations_BreakDurationId",
                table: "WorkTimes");

            migrationBuilder.DropTable(
                name: "BreakDurations");

            migrationBuilder.DropIndex(
                name: "IX_WorkTimes_BreakDurationId",
                table: "WorkTimes");

            migrationBuilder.DropColumn(
                name: "BreakDurationId",
                table: "WorkTimes");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "WorkTimes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "WorkTimes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "AllowedBreakTime",
                table: "WorkTimes",
                type: "time without time zone",
                nullable: true);
        }
    }
}
