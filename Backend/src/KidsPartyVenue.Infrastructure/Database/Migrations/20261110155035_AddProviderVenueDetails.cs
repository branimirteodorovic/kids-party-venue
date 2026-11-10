using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidsPartyVenue.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderVenueDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "venue_food_service",
                table: "providers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "venue_has_own_animator",
                table: "providers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "venue_has_own_photographer",
                table: "providers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "venue_has_smoking_area",
                table: "providers",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "venue_max_adults",
                table: "providers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "venue_max_age",
                table: "providers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "venue_max_children",
                table: "providers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "venue_min_age",
                table: "providers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "venue_outside_cake",
                table: "providers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "venue_outside_catering",
                table: "providers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_providers_venue_age_range",
                table: "providers",
                sql: "venue_min_age <= venue_max_age");

            migrationBuilder.AddCheckConstraint(
                name: "ck_providers_venue_details_category",
                table: "providers",
                sql: "category = 'Venue' OR venue_max_children IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_providers_venue_details_complete",
                table: "providers",
                sql: "(venue_max_children IS NULL AND venue_max_adults IS NULL AND venue_min_age IS NULL\n    AND venue_max_age IS NULL AND venue_outside_catering IS NULL AND venue_outside_cake IS NULL\n    AND venue_food_service IS NULL AND venue_has_own_animator IS NULL\n    AND venue_has_own_photographer IS NULL AND venue_has_smoking_area IS NULL)\nOR (venue_max_children IS NOT NULL AND venue_min_age IS NOT NULL AND venue_max_age IS NOT NULL\n    AND venue_outside_catering IS NOT NULL AND venue_outside_cake IS NOT NULL\n    AND venue_food_service IS NOT NULL AND venue_has_own_animator IS NOT NULL\n    AND venue_has_own_photographer IS NOT NULL AND venue_has_smoking_area IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_providers_venue_age_range",
                table: "providers");

            migrationBuilder.DropCheckConstraint(
                name: "ck_providers_venue_details_category",
                table: "providers");

            migrationBuilder.DropCheckConstraint(
                name: "ck_providers_venue_details_complete",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_food_service",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_has_own_animator",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_has_own_photographer",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_has_smoking_area",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_max_adults",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_max_age",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_max_children",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_min_age",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_outside_cake",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "venue_outside_catering",
                table: "providers");
        }
    }
}
