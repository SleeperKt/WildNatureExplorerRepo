using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WildNatureExplorer.Infrastructure.Migrations
{
    /// <summary>
    /// Adds the personal "User Library" feature:
    ///   * UserSightings table (each animal saved by a user with coords + photo).
    ///   * Spatial GIST index on (Longitude, Latitude) for radius queries.
    ///   * PostgreSQL function fn_user_nearby_sightings(user_id, lat, lng, radius_km)
    ///     that returns the user's saved sightings inside the radius, ordered by distance.
    /// </summary>
    public partial class AddUserLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ----------------------------------------------------------------
            // 1. PostGIS extension – required for ST_DWithin / ST_Distance
            // ----------------------------------------------------------------
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS postgis;");

            // ----------------------------------------------------------------
            // 2. UserSightings table
            // ----------------------------------------------------------------
            migrationBuilder.CreateTable(
                name: "UserSightings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpeciesId = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SightedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSightings", x => x.Id);

                    table.ForeignKey(
                        name: "FK_UserSightings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_UserSightings_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // CHECK constraints on lat/lng (the entity also enforces them, this is the DB safety net)
            migrationBuilder.Sql(@"
                ALTER TABLE ""UserSightings""
                    ADD CONSTRAINT ""CK_UserSightings_Latitude_Range""
                    CHECK (""Latitude"" BETWEEN -90 AND 90);

                ALTER TABLE ""UserSightings""
                    ADD CONSTRAINT ""CK_UserSightings_Longitude_Range""
                    CHECK (""Longitude"" BETWEEN -180 AND 180);
            ");

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_UserSightings_UserId",
                table: "UserSightings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSightings_UserId_SightedAt",
                table: "UserSightings",
                columns: new[] { "UserId", "SightedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSightings_SpeciesId",
                table: "UserSightings",
                column: "SpeciesId");

            migrationBuilder.CreateIndex(
                name: "UX_UserSightings_User_Species_SightedAt",
                table: "UserSightings",
                columns: new[] { "UserId", "SpeciesId", "SightedAt" },
                unique: true);

            // Spatial GIST index on the geography point for fast radius queries.
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_UserSightings_Geo""
                    ON ""UserSightings""
                    USING GIST (geography(ST_SetSRID(ST_MakePoint(""Longitude"", ""Latitude""), 4326)));
            ");

            // Reverse spatial index on SpeciesLocations too — also needed by the
            // existing `simulate_path_with_dangers` function and a recurring audit gap.
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_SpeciesLocations_Geo""
                    ON ""SpeciesLocations""
                    USING GIST (geography(ST_SetSRID(ST_MakePoint(""Longitude"", ""Latitude""), 4326)));
            ");

            // ----------------------------------------------------------------
            // 3. fn_user_nearby_sightings — the User Library "find animals around me" function.
            //
            //    Returns every sighting the user owns within p_radius_km of
            //    (p_lat, p_lng), enriched with species info and the exact great-circle
            //    distance, ordered by distance ascending.
            // ----------------------------------------------------------------
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION fn_user_nearby_sightings(
                    p_user_id     uuid,
                    p_lat         double precision,
                    p_lng         double precision,
                    p_radius_km   numeric
                )
                RETURNS TABLE (
                    sighting_id      uuid,
                    species_id       uuid,
                    common_name      text,
                    scientific_name  text,
                    is_dangerous     boolean,
                    is_rare          boolean,
                    latitude         double precision,
                    longitude        double precision,
                    image_url        text,
                    notes            text,
                    sighted_at       timestamptz,
                    created_at       timestamptz,
                    distance_km      numeric
                )
                LANGUAGE plpgsql
                STABLE
                AS $$
                DECLARE
                    v_origin geography := geography(ST_SetSRID(ST_MakePoint(p_lng, p_lat), 4326));
                    v_radius_m double precision := (p_radius_km * 1000.0)::double precision;
                BEGIN
                    IF p_user_id IS NULL THEN
                        RAISE EXCEPTION 'p_user_id is required';
                    END IF;

                    IF p_lat NOT BETWEEN -90 AND 90 THEN
                        RAISE EXCEPTION 'p_lat must be between -90 and 90, got %', p_lat;
                    END IF;

                    IF p_lng NOT BETWEEN -180 AND 180 THEN
                        RAISE EXCEPTION 'p_lng must be between -180 and 180, got %', p_lng;
                    END IF;

                    IF p_radius_km IS NULL OR p_radius_km <= 0 OR p_radius_km > 1000 THEN
                        RAISE EXCEPTION 'p_radius_km must be in (0, 1000], got %', p_radius_km;
                    END IF;

                    RETURN QUERY
                    SELECT
                        us.""Id""               AS sighting_id,
                        sp.""Id""               AS species_id,
                        sp.""CommonName""::text AS common_name,
                        sp.""ScientificName""::text AS scientific_name,
                        sp.""IsDangerous""      AS is_dangerous,
                        sp.""IsRare""           AS is_rare,
                        us.""Latitude""         AS latitude,
                        us.""Longitude""        AS longitude,
                        us.""ImageUrl""::text   AS image_url,
                        us.""Notes""::text      AS notes,
                        us.""SightedAt""        AS sighted_at,
                        us.""CreatedAt""        AS created_at,
                        ROUND(
                            (ST_Distance(
                                geography(ST_SetSRID(ST_MakePoint(us.""Longitude"", us.""Latitude""), 4326)),
                                v_origin
                            ) / 1000.0)::numeric,
                            3
                        ) AS distance_km
                    FROM ""UserSightings"" us
                    JOIN ""Species"" sp ON sp.""Id"" = us.""SpeciesId""
                    WHERE us.""UserId"" = p_user_id
                      AND ST_DWithin(
                            geography(ST_SetSRID(ST_MakePoint(us.""Longitude"", us.""Latitude""), 4326)),
                            v_origin,
                            v_radius_m
                          )
                    ORDER BY distance_km ASC;
                END;
                $$;

                COMMENT ON FUNCTION fn_user_nearby_sightings(uuid, double precision, double precision, numeric)
                    IS 'Returns sightings owned by p_user_id within p_radius_km of (p_lat, p_lng), ordered by distance.';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP FUNCTION IF EXISTS fn_user_nearby_sightings(uuid, double precision, double precision, numeric);
                DROP INDEX IF EXISTS ""IX_UserSightings_Geo"";
                DROP INDEX IF EXISTS ""IX_SpeciesLocations_Geo"";
            ");

            migrationBuilder.DropTable(name: "UserSightings");

            // PostGIS extension is intentionally left installed.
        }
    }
}
