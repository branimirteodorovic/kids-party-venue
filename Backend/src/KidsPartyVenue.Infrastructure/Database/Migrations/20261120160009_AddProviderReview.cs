using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KidsPartyVenue.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "review_notes",
                table: "providers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "reviewed_at_utc",
                table: "providers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "submitted_at_utc",
                table: "providers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "suspension_reason",
                table: "providers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_providers_suspension",
                table: "providers",
                sql: "(suspended_at_utc IS NULL) = (suspension_reason IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_providers_suspension",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "review_notes",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "reviewed_at_utc",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "submitted_at_utc",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "suspension_reason",
                table: "providers");
        }
    }
}
