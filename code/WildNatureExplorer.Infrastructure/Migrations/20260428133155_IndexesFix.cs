using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WildNatureExplorer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IndexesFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SpeciesLocations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "SpeciesLocations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Species",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Species",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Sizes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Sizes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Roles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Roles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Habitats",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Habitats",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Countries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Countries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Colors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Colors",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AiSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AiMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AiFeedbacks",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsActive",
                table: "Users",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_AssignedAt",
                table: "UserRoles",
                column: "AssignedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesLocations_CreatedAt",
                table: "SpeciesLocations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesHabitats_SpeciesId",
                table: "SpeciesHabitats",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesCountries_SpeciesId",
                table: "SpeciesCountries",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesColors_SpeciesId",
                table: "SpeciesColors",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "IX_Species_CreatedAt",
                table: "Species",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreatedAt",
                table: "Roles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiSessions_CreatedAt",
                table: "AiSessions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiSessions_UserId",
                table: "AiSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AiMessages_CreatedAt",
                table: "AiMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiFeedbacks_CreatedAt",
                table: "AiFeedbacks",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AiFeedbacks_SessionId",
                table: "AiFeedbacks",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AiFeedbacks_AiSessions_SessionId",
                table: "AiFeedbacks",
                column: "SessionId",
                principalTable: "AiSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AiSessions_Users_UserId",
                table: "AiSessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiFeedbacks_AiSessions_SessionId",
                table: "AiFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_AiSessions_Users_UserId",
                table: "AiSessions");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsActive",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_AssignedAt",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_UserId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_SpeciesLocations_CreatedAt",
                table: "SpeciesLocations");

            migrationBuilder.DropIndex(
                name: "IX_SpeciesHabitats_SpeciesId",
                table: "SpeciesHabitats");

            migrationBuilder.DropIndex(
                name: "IX_SpeciesCountries_SpeciesId",
                table: "SpeciesCountries");

            migrationBuilder.DropIndex(
                name: "IX_SpeciesColors_SpeciesId",
                table: "SpeciesColors");

            migrationBuilder.DropIndex(
                name: "IX_Species_CreatedAt",
                table: "Species");

            migrationBuilder.DropIndex(
                name: "IX_Roles_CreatedAt",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_AiSessions_CreatedAt",
                table: "AiSessions");

            migrationBuilder.DropIndex(
                name: "IX_AiSessions_UserId",
                table: "AiSessions");

            migrationBuilder.DropIndex(
                name: "IX_AiMessages_CreatedAt",
                table: "AiMessages");

            migrationBuilder.DropIndex(
                name: "IX_AiFeedbacks_CreatedAt",
                table: "AiFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_AiFeedbacks_SessionId",
                table: "AiFeedbacks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SpeciesLocations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SpeciesLocations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Species");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Species");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Sizes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Sizes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Habitats");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Habitats");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AiSessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AiMessages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AiFeedbacks");
        }
    }
}
