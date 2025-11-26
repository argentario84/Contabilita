using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contabilita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequireDescriptionAndIsShared : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequireDescription",
                table: "Categories",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "CalendarEvents",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequireDescription",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "CalendarEvents");
        }
    }
}
