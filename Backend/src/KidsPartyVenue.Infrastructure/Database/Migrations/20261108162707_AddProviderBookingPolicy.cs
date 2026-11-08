using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidsPartyVenue.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderBookingPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "booking_deposit_due_days",
                table: "providers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "booking_deposit_type",
                table: "providers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "booking_deposit_value",
                table: "providers",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "booking_free_cancellation_days",
                table: "providers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_providers_booking_deposit",
                table: "providers",
                sql: "booking_deposit_type IS NULL\nOR (booking_deposit_type = 'None' AND booking_deposit_value IS NULL AND booking_deposit_due_days IS NULL)\nOR (booking_deposit_type IN ('Percentage', 'FixedAmount')\n    AND booking_deposit_value IS NOT NULL AND booking_deposit_due_days IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_providers_booking_policy_complete",
                table: "providers",
                sql: "(booking_deposit_type IS NULL AND booking_deposit_value IS NULL\n    AND booking_deposit_due_days IS NULL AND booking_free_cancellation_days IS NULL)\nOR (booking_deposit_type IS NOT NULL AND booking_free_cancellation_days IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_providers_booking_deposit",
                table: "providers");

            migrationBuilder.DropCheckConstraint(
                name: "ck_providers_booking_policy_complete",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "booking_deposit_due_days",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "booking_deposit_type",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "booking_deposit_value",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "booking_free_cancellation_days",
                table: "providers");
        }
    }
}
