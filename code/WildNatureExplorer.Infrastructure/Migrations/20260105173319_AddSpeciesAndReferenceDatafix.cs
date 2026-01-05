using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WildNatureExplorer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeciesAndReferenceDatafix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Habitats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habitats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sizes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Species",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommonName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ScientificName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsDangerous = table.Column<bool>(type: "boolean", nullable: false),
                    IsRare = table.Column<bool>(type: "boolean", nullable: false),
                    SizeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Species", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Species_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesColors",
                columns: table => new
                {
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    ColorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesColors", x => new { x.SpeciesId, x.ColorId });
                    table.ForeignKey(
                        name: "FK_SpeciesColors_Colors_ColorId",
                        column: x => x.ColorId,
                        principalTable: "Colors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeciesColors_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesCountries",
                columns: table => new
                {
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesCountries", x => new { x.SpeciesId, x.CountryId });
                    table.ForeignKey(
                        name: "FK_SpeciesCountries_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeciesCountries_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesHabitats",
                columns: table => new
                {
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    HabitatId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesHabitats", x => new { x.SpeciesId, x.HabitatId });
                    table.ForeignKey(
                        name: "FK_SpeciesHabitats_Habitats_HabitatId",
                        column: x => x.HabitatId,
                        principalTable: "Habitats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpeciesHabitats_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeciesLocations_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Colors_NormalizedName",
                table: "Colors",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Countries_NormalizedName",
                table: "Countries",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Habitats_NormalizedName",
                table: "Habitats",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_NormalizedName",
                table: "Sizes",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Species_SizeId",
                table: "Species",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesColors_ColorId",
                table: "SpeciesColors",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesCountries_CountryId",
                table: "SpeciesCountries",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesHabitats_HabitatId",
                table: "SpeciesHabitats",
                column: "HabitatId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesLocations_SpeciesId",
                table: "SpeciesLocations",
                column: "SpeciesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpeciesColors");

            migrationBuilder.DropTable(
                name: "SpeciesCountries");

            migrationBuilder.DropTable(
                name: "SpeciesHabitats");

            migrationBuilder.DropTable(
                name: "SpeciesLocations");

            migrationBuilder.DropTable(
                name: "Colors");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Habitats");

            migrationBuilder.DropTable(
                name: "Species");

            migrationBuilder.DropTable(
                name: "Sizes");
        }
    }
}
