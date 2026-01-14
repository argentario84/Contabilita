using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contabilita.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetPlanningFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SavingsGoalAmount",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SavingsGoalPercentage",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseSavingsPercentage",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraFixedExpenses",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BudgetAlertThreshold",
                table: "AspNetUsers",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 80m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SavingsGoalAmount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SavingsGoalPercentage",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UseSavingsPercentage",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ExtraFixedExpenses",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BudgetAlertThreshold",
                table: "AspNetUsers");
        }
    }
}
